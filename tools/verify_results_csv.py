#!/usr/bin/env python3
"""
Independent verifier for an EPR Calculator "Results" CSV export.

WHAT THIS CHECKS
================
The Results CSV is a single wide export containing (amongst other things) a
"Calculation Result" table with one row per producer/subsidiary. This script
re-derives, from first principles, the figures in that table for each
Level-1 (L1) producer row -- i.e. the per-producer total used for billing,
as opposed to the underlying L2 subsidiary rows or the single "all producers"
grand-total row -- and reports any row where the file's own printed figures
don't match what the documented calculation would produce.

Every number used below is read from the Results CSV itself. Nothing is
recomputed from a separate database, submission file, or parameter sheet, so
this script only tells you "is this file internally consistent with its own
documented formulas?" -- it cannot tell you whether the *inputs* fed into the
run were themselves correct.

For each L1 producer row, the checks are:

  1. Modulation pricing (run-wide, checked once): the "Modulation Calculation"
     section derives a Red and Green disposal-cost-per-tonne for each
     material from a single Amber ("flat") price plus a Red Modulation
     Factor. We recompute the Green Modulation Factor and the per-material
     Red/Green prices from that section's own printed intermediate figures,
     and separately confirm that section's starting tonnage figures equal
     the sum of every L1 producer's own Net Tonnage plus the Late Reporting
     Tonnage parameter -- tying the run-wide pricing back to the same
     per-producer data checked in step 2, rather than trusting it in isolation.

  2. Section 1 -- LA Disposal Fee: for each material, the producer's own
     "Net Tonnage" (Red/Amber/Green) is multiplied by that material's
     Red/Amber/Green price-per-tonne (from check 1), bad debt provision is
     added, and the result is apportioned across England/Wales/Scotland/
     Northern Ireland. We recompute this per material and re-sum it to the
     producer's Section-1 total. This also replicates the app's zero-override
     guard (a material's fee is forced to zero if self-managed consumer
     waste tonnage exceeds raw reported tonnage): for a multi-entity producer
     that comparison is against group-level, summed-across-L2 figures, but
     the app prints those already-aggregated figures directly on the L1 row
     itself (ProducerRowBuilder.GetL1TotalRow), so no L2 data is needed to
     replicate it for single-entity or multi-entity producers alike.

  3. Sections 2a/2b/2c -- Comms Costs: 2a multiplies the producer's raw
     reported tonnage by a per-material comms price; 2b and 2c apportion the
     run-wide UK-wide and by-country comms cost totals to the producer by its
     share of all producers' tonnage. All three add bad debt provision and
     apportion by country (2a and 2b via the 1+4 apportionment, 2c via the
     raw by-country split), using the "Parameters - Comms Costs" section's
     own printed price/cost figures as inputs.

  4. Sections 3/4/5 -- SA Operating Costs, LA Data Prep Costs, SA Set Up
     Costs: each is a fixed run-wide total (from "Parameters - Other")
     apportioned to the producer by its own "Percentage of Overall Producer
     Cost for (1+2a+2b+2c)" -- which we also independently recompute, as that
     producer's own (1+2a+2b+2c) total over the run-wide total printed once
     in the file's header row -- then split by country (3 and 5 via 1+4%,
     4 via its own "4 Country Apportionment %s").

  5. Total Producer Bill: Section 1 through 5 (all independently recomputed
     above) should sum to the printed Total Producer Bill, both in total and
     per country.

  6. Suggested Billing Instruction: given the Total Producer Bill (with bad
     debt provision) and the producer's prior invoiced total, we recompute
     the liability difference, the materiality/tonnage-change threshold
     flags, the suggested instruction (INITIAL/DELTA/REBILL/-), and the
     suggested invoice amount.

SCOPE / KNOWN LIMITATIONS
=========================
  * L1 rows only. L2 subsidiary rows and the single overall-total row are
    read (for context) but not verified.
  * Requires a Results file that includes the "Modulation Calculation"
    section (i.e. a run using RAG-rating disposal pricing). Files without
    that section are pre-modulation and are not supported -- the script
    will say so rather than silently checking the wrong formula.

USAGE
=====
    python3 verify_results_csv.py path/to/2026-results.csv
    python3 verify_results_csv.py path/to/2026-results.csv --verbose
    python3 verify_results_csv.py path/to/2026-results.csv --tolerance 0.02
"""

from __future__ import annotations

import argparse
import csv
import sys
from dataclasses import dataclass, field
from decimal import Decimal, ROUND_HALF_UP
from typing import Optional


# ---------------------------------------------------------------------------
# Low-level parsing helpers
#
# The Results CSV renders every number as a formatted string (currency with
# a "£" prefix, percentages with a "%" suffix, or a plain decimal), and uses
# "-" for values that don't apply to a row. These helpers undo that
# formatting back into Decimal (or None), and reproduce the file's own
# "round half away from zero" rounding so that a value we recompute and a
# value we read out of the file round to the same printed string.
# ---------------------------------------------------------------------------

HYPHEN = "-"


def round_half_away_from_zero(value: Decimal, places: int) -> Decimal:
    """Matches .NET's MidpointRounding.AwayFromZero, used throughout the C# exporter."""
    quantum = Decimal(1).scaleb(-places)
    return value.quantize(quantum, rounding=ROUND_HALF_UP)


def parse_decimal(raw: str) -> Optional[Decimal]:
    """Parses a plain (non-currency, non-percentage) number cell. '-' and '' -> None."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    return Decimal(text.replace(",", ""))


def parse_money(raw: str) -> Optional[Decimal]:
    """Parses a '£123.45' / '-£123.45' cell. '-' and '' -> None."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    negative = text.startswith("-")
    text = text.lstrip("-").replace("£", "").replace(",", "")
    value = Decimal(text)
    return -value if negative else value


def parse_percent(raw: str) -> Optional[Decimal]:
    """Parses a '54.04873246%' cell into the number 54.04873246 (not 0.5404...)."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    negative = text.startswith("-")
    text = text.lstrip("-").replace("%", "").replace(",", "")
    value = Decimal(text)
    return -value if negative else value


def d(value) -> Decimal:
    """Decimal(...) that also accepts None as zero, for terser arithmetic below."""
    return Decimal(0) if value is None else Decimal(value)


# ---------------------------------------------------------------------------
# Whole-file loading and section lookup
# ---------------------------------------------------------------------------


def load_rows(path: str) -> list[list[str]]:
    with open(path, newline="", encoding="utf-8-sig") as f:
        return list(csv.reader(f))


def find_row(rows: list[list[str]], first_cell: str, start: int = 0) -> int:
    """Index of the first row whose first cell equals `first_cell`, searching from `start`."""
    for i in range(start, len(rows)):
        if rows[i] and rows[i][0].strip() == first_cell:
            return i
    raise ValueError(f"Section {first_cell!r} not found in file (searching from row {start})")


def find_row_containing(rows: list[list[str]], cell_text: str, start: int = 0) -> int:
    """Index of the first row that contains `cell_text` in any column."""
    for i in range(start, len(rows)):
        if cell_text in rows[i]:
            return i
    raise ValueError(f"No row containing {cell_text!r} found (searching from row {start})")


# ---------------------------------------------------------------------------
# Country apportionment (England/Wales/Scotland/Northern Ireland)
# ---------------------------------------------------------------------------


@dataclass
class ByCountry:
    england: Decimal
    wales: Decimal
    scotland: Decimal
    northern_ireland: Decimal

    @property
    def total(self) -> Decimal:
        return self.england + self.wales + self.scotland + self.northern_ireland

    def __add__(self, other: "ByCountry") -> "ByCountry":
        return ByCountry(
            self.england + other.england,
            self.wales + other.wales,
            self.scotland + other.scotland,
            self.northern_ireland + other.northern_ireland,
        )


# ---------------------------------------------------------------------------
# Section parsers: pull run-wide (not per-producer) figures out of the
# header portion of the file, before the per-producer table begins.
# ---------------------------------------------------------------------------


@dataclass
class LapcapData:
    materials_order: list[str]
    country_apportionment_pct: ByCountry  # "1 Country Apportionment %s"


def parse_lapcap_data(rows: list[list[str]]) -> LapcapData:
    section = find_row(rows, "LAPCAP Data")
    header_row = section + 1
    materials = []
    r = header_row + 1
    while rows[r][0].strip() != "Total":
        materials.append(rows[r][0].strip())
        r += 1
    total_row = r
    apportionment_row = rows[total_row + 1]
    assert apportionment_row[0].strip() == "1 Country Apportionment %s", apportionment_row[0]
    apportionment = ByCountry(
        england=parse_percent(apportionment_row[1]),
        wales=parse_percent(apportionment_row[2]),
        scotland=parse_percent(apportionment_row[3]),
        northern_ireland=parse_percent(apportionment_row[4]),
    )
    return LapcapData(materials_order=materials, country_apportionment_pct=apportionment)


@dataclass
class LaDisposalCostData:
    # material name -> Disposal Cost Price Per Tonne (the "Amber"/flat price)
    price_per_tonne: dict[str, Decimal]


def parse_la_disposal_cost_data(rows: list[list[str]], materials_order: list[str]) -> LaDisposalCostData:
    section = find_row(rows, "LA Disposal Cost Data")
    header = rows[section + 1]
    price_col = header.index("Disposal Cost Price Per Tonne")
    prices = {}
    r = section + 2
    for _ in materials_order:
        material = rows[r][0].strip()
        prices[material] = parse_money(rows[r][price_col])
        r += 1
    return LaDisposalCostData(price_per_tonne=prices)


@dataclass
class RagAmounts:
    red: Decimal
    amber: Decimal
    green: Decimal


@dataclass
class LateReportingTonnages:
    # material name -> grouped (Red+RedMedical, Amber+AmberMedical, Green+GreenMedical) late reporting tonnage
    by_material: dict[str, RagAmounts]


def parse_late_reporting_tonnages(rows: list[list[str]], materials_order: list[str]) -> LateReportingTonnages:
    section = find_row(rows, "Parameters - Late Reporting Tonnages")
    header = rows[section + 1]
    col = {name: i for i, name in enumerate(header)}
    by_material = {}
    r = section + 2
    for _ in materials_order:
        cells = rows[r]
        material = cells[0].strip()
        by_material[material] = RagAmounts(
            red=parse_decimal(cells[col["Red + Red Medical Late Reporting Tonnage"]]),
            amber=parse_decimal(cells[col["Amber + Amber Medical Late Reporting Tonnage"]]),
            green=parse_decimal(cells[col["Green + Green Medical Late Reporting Tonnage"]]),
        )
        r += 1
    return LateReportingTonnages(by_material=by_material)


@dataclass
class MaterialModulation:
    red_net_tonnage: Decimal
    amber_net_tonnage: Decimal
    green_net_tonnage: Decimal
    total_red_at_amber_cost: Decimal
    total_green_at_amber_cost: Decimal
    red_price: Decimal
    amber_price: Decimal
    green_price: Decimal


@dataclass
class ModulationCalculation:
    red_factor: Decimal
    green_factor_printed: Decimal
    by_material: dict[str, MaterialModulation]
    total_red_at_amber_cost: Decimal
    total_green_at_amber_cost: Decimal


def parse_modulation_calculation(rows: list[list[str]], materials_order: list[str]) -> Optional[ModulationCalculation]:
    try:
        section = find_row(rows, "Modulation Calculation")
    except ValueError:
        return None

    red_factor = parse_decimal(rows[section + 1][1])
    green_factor_printed = parse_decimal(rows[section + 2][1])
    header_row = section + 3
    header = rows[header_row]
    col = {name: i for i, name in enumerate(header)}

    by_material: dict[str, MaterialModulation] = {}
    r = header_row + 1
    for _ in materials_order:
        cells = rows[r]
        material = cells[0].strip()
        by_material[material] = MaterialModulation(
            red_net_tonnage=parse_decimal(cells[col["Red + Red Medical Net Tonnage + Late Reporting Tonnage"]]),
            amber_net_tonnage=parse_decimal(cells[col["Amber + Amber Medical Net Tonnage + Late Reporting Tonnage"]]),
            green_net_tonnage=parse_decimal(cells[col["Green + Green Medical Net Tonnage + Late Reporting Tonnage"]]),
            total_red_at_amber_cost=parse_money(
                cells[col["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]
            ),
            total_green_at_amber_cost=parse_money(
                cells[col["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]
            ),
            red_price=parse_money(cells[col["Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost"]]),
            amber_price=parse_money(cells[col["Amber Material Disposal Cost = Material Disposal Cost per Tonne"]]),
            green_price=parse_money(cells[col["Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"]]),
        )
        r += 1

    total_row = rows[r]
    assert total_row[0].strip() == "Total", total_row[0]
    total_red_at_amber = parse_money(
        total_row[col["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]
    )
    total_green_at_amber = parse_money(
        total_row[col["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]
    )

    return ModulationCalculation(
        red_factor=red_factor,
        green_factor_printed=green_factor_printed,
        by_material=by_material,
        total_red_at_amber_cost=total_red_at_amber,
        total_green_at_amber_cost=total_green_at_amber,
    )


@dataclass
class Materiality:
    amount: Decimal
    percentage: Decimal


@dataclass
class OtherParameters:
    sa_operating_cost_total: Decimal  # "3 SA Operating Costs" Total column
    la_data_prep_charge_total: Decimal  # "4 LA Data Prep Charge" Total column
    la_data_prep_apportionment_pct: ByCountry  # "4 Country Apportionment %s"
    scheme_setup_cost_total: Decimal  # "5 Scheme set up cost Yearly Cost" Total column
    bad_debt_pct: Decimal
    materiality_increase: Materiality
    materiality_decrease: Materiality
    tonnage_change_increase: Materiality
    tonnage_change_decrease: Materiality


def parse_other_parameters(rows: list[list[str]]) -> OtherParameters:
    sa_operating_row = rows[find_row(rows, "3 SA Operating Costs")]
    sa_operating_cost_total = parse_money(sa_operating_row[5])

    la_data_prep_row_idx = find_row(rows, "4 LA Data Prep Charge")
    la_data_prep_charge_total = parse_money(rows[la_data_prep_row_idx][5])
    la_data_prep_apportionment_row = rows[la_data_prep_row_idx + 1]
    assert la_data_prep_apportionment_row[0].strip() == "4 Country Apportionment %s", la_data_prep_apportionment_row[0]
    la_data_prep_apportionment_pct = ByCountry(
        england=parse_percent(la_data_prep_apportionment_row[1]),
        wales=parse_percent(la_data_prep_apportionment_row[2]),
        scotland=parse_percent(la_data_prep_apportionment_row[3]),
        northern_ireland=parse_percent(la_data_prep_apportionment_row[4]),
    )

    scheme_setup_row = rows[find_row(rows, "5 Scheme set up cost Yearly Cost")]
    scheme_setup_cost_total = parse_money(scheme_setup_row[5])

    bad_debt_row = find_row(rows, "6 Bad Debt Provision")
    bad_debt_pct = parse_percent(rows[bad_debt_row][1])

    mat_row = find_row(rows, "7 Materiality")
    mat_increase = rows[mat_row + 1]
    mat_decrease = rows[mat_row + 2]
    assert mat_increase[0].strip() == "Increase" and mat_decrease[0].strip() == "Decrease"

    ton_row = find_row(rows, "8 Tonnage Change")
    ton_increase = rows[ton_row + 1]
    ton_decrease = rows[ton_row + 2]
    assert ton_increase[0].strip() == "Increase" and ton_decrease[0].strip() == "Decrease"

    return OtherParameters(
        sa_operating_cost_total=sa_operating_cost_total,
        la_data_prep_charge_total=la_data_prep_charge_total,
        la_data_prep_apportionment_pct=la_data_prep_apportionment_pct,
        scheme_setup_cost_total=scheme_setup_cost_total,
        bad_debt_pct=bad_debt_pct,
        materiality_increase=Materiality(parse_money(mat_increase[1]), parse_percent(mat_increase[2])),
        materiality_decrease=Materiality(parse_money(mat_decrease[1]), parse_percent(mat_decrease[2])),
        tonnage_change_increase=Materiality(parse_money(ton_increase[1]), parse_percent(ton_increase[2])),
        tonnage_change_decrease=Materiality(parse_money(ton_decrease[1]), parse_percent(ton_decrease[2])),
    )


@dataclass
class CommsCostParameters:
    one_plus_four_apportionment_pct: ByCountry  # "1 + 4 Apportionment %s" (within Parameters - Comms Costs)
    price_per_tonne_by_material: dict[str, Decimal]  # "2a Comms Costs - by Material" -- price per tonne column
    uk_wide: ByCountry  # "2b Comms Costs - UK wide" row (before bad debt)
    by_country: ByCountry  # "2c Comms Costs - by Country" row (before bad debt)


def parse_comms_cost_parameters(rows: list[list[str]], materials_order: list[str]) -> CommsCostParameters:
    section = find_row(rows, "Parameters - Comms Costs")
    apportionment_row = rows[find_row(rows, "1 + 4 Apportionment %s", start=section)]
    one_plus_four_apportionment_pct = ByCountry(
        england=parse_percent(apportionment_row[1]),
        wales=parse_percent(apportionment_row[2]),
        scotland=parse_percent(apportionment_row[3]),
        northern_ireland=parse_percent(apportionment_row[4]),
    )

    header_row = find_row(rows, "2a Comms Costs - by Material", start=section)
    header = rows[header_row]
    price_col = header.index("Comms Cost - by Material Price Per Tonne")
    price_per_tonne_by_material = {}
    r = header_row + 1
    for _ in materials_order:
        material = rows[r][0].strip()
        price_per_tonne_by_material[material] = parse_money(rows[r][price_col])
        r += 1

    uk_wide_row = rows[find_row(rows, "2b Comms Costs - UK wide", start=section)]
    uk_wide = ByCountry(
        england=d(parse_money(uk_wide_row[1])), wales=d(parse_money(uk_wide_row[2])),
        scotland=d(parse_money(uk_wide_row[3])), northern_ireland=d(parse_money(uk_wide_row[4])),
    )

    by_country_row = rows[find_row(rows, "2c Comms Costs - by Country", start=section)]
    by_country = ByCountry(
        england=d(parse_money(by_country_row[1])), wales=d(parse_money(by_country_row[2])),
        scotland=d(parse_money(by_country_row[3])), northern_ireland=d(parse_money(by_country_row[4])),
    )

    return CommsCostParameters(
        one_plus_four_apportionment_pct=one_plus_four_apportionment_pct,
        price_per_tonne_by_material=price_per_tonne_by_material,
        uk_wide=uk_wide,
        by_country=by_country,
    )


# ---------------------------------------------------------------------------
# Main per-producer table: column schema.
#
# The table is a flat sequence of fixed-width blocks (one per exporter class
# in the real C# CalcResultSummaryExporter), some of which repeat once per
# material. We rebuild the exact same sequence of blocks here so that we can
# slice each producer's row into named fields -- this mirrors how the file
# is *written*, which is more robust than trying to guess field boundaries
# from the (highly repetitive, not-fully-unique) header text alone.
# ---------------------------------------------------------------------------

RAG_KEYS = ["Red", "Amber", "Green", "RedMedical", "AmberMedical", "GreenMedical"]
RAG_GROUPS = ["Red", "Amber", "Green"]  # RedMedical groups with Red, etc.


@dataclass
class Block:
    name: str
    width: int


def build_producer_table_schema(materials_order: list[str], glass_name: Optional[str]) -> list[Block]:
    def is_glass(material: str) -> bool:
        return glass_name is not None and material == glass_name

    blocks: list[Block] = [Block("identity", 10)]

    for material in materials_order:
        # 1 (prev invoiced)
        # + household (1 + 6 rag)
        # + public bin (1 + 6 rag)
        # + [glass: hdc (1 + 6 rag)]
        # + total-tonnage block: total(1) + rag(6) + grouped-rag(3) = 10
        # + smcw(1) + actioned-smcw(total,red,amber,green = 4)
        # + net-tonnage(total,red,amber,green = 4)
        # + residual smcw(1)
        # + tonnage change(1)
        # + red/amber/green price(3) + red/amber/green disposal cost(3)
        # + fee w/o bdp(1) + bdp(1) + fee w/ bdp total+4 countries(5)
        width = 1 + (1 + 6) + (1 + 6) + (10) + (1 + 4) + 4 + 1 + 1 + 6 + 7
        if is_glass(material):
            width += 1 + 6
        blocks.append(Block(f"section1::{material}", width))

    blocks.append(Block("section1_total", 9))

    for material in materials_order:
        width = 2 + (1 if is_glass(material) else 0) + 9  # hh,pb,[hdc],total,price,fee,bdp,fee+4countries
        blocks.append(Block(f"section2a::{material}", width))

    blocks.append(Block("section2a_total_a", 7))  # Section2aCommsExporter
    blocks.append(Block("section1_total_b", 7))  # Section1DisposalExporter (repeat)
    blocks.append(Block("section2a_total_b", 7))  # Section2aComms2aExporter (repeat)
    blocks.append(Block("pct_vs_all_producers", 1))
    blocks.append(Block("section2b_total", 7))
    blocks.append(Block("section2c_total", 7))
    blocks.append(Block("oneplus_2a2b2c", 2))
    blocks.append(Block("section3_total", 7))
    blocks.append(Block("section4_total", 7))
    blocks.append(Block("section5_total", 7))
    blocks.append(Block("total_bill", 7))
    blocks.append(Block("billing_instructions", 10))
    return blocks


def block_offsets(blocks: list[Block]) -> dict[str, tuple[int, int]]:
    offsets = {}
    pos = 0
    for b in blocks:
        offsets[b.name] = (pos, pos + b.width)
        pos += b.width
    return offsets


# ---------------------------------------------------------------------------
# Per-producer parsed data
# ---------------------------------------------------------------------------


@dataclass
class WithBdp:
    """
    The recurring 7-cell group the exporter writes for every fee section:
    '<x> w/o Bad Debt Provision', 'Bad Debt Provision', '<x> with Bad Debt
    Provision' (a standalone total), then England/Wales/Scotland/Northern
    Ireland (each already 'with Bad Debt Provision').

    `total` is read directly from its own printed cell rather than derived by
    summing `by_country` -- the file rounds the total and each of the four
    country shares *independently* for display, so summing the (rounded)
    country cells can be a penny or two off from the (separately rounded)
    printed total. Keeping both as printed preserves that distinction.
    """
    without_bdp: Decimal
    bdp: Decimal
    total: Decimal
    by_country: ByCountry


def by_country_from_cells(cells: list[str]) -> ByCountry:
    """Reads 4 consecutive England/Wales/Scotland/NI currency cells."""
    return ByCountry(
        england=d(parse_money(cells[0])),
        wales=d(parse_money(cells[1])),
        scotland=d(parse_money(cells[2])),
        northern_ireland=d(parse_money(cells[3])),
    )


def parse_with_bdp(cells: list[str], i: int) -> tuple[WithBdp, int]:
    """Parses a WithBdp starting at index i, returning it plus the index just past it."""
    without_bdp = d(parse_money(cells[i])); i += 1
    bdp = d(parse_money(cells[i])); i += 1
    total = d(parse_money(cells[i])); i += 1
    by_country = by_country_from_cells(cells[i:i + 4]); i += 4
    return WithBdp(without_bdp, bdp, total, by_country), i


@dataclass
class MaterialFigures:
    raw_total_tonnage: Optional[Decimal]  # "Total Tonnage" -- reported tonnage before SMCW is deducted
    smcw_tonnage: Optional[Decimal]  # "Self Managed Consumer Waste Tonnage"
    net_red: Optional[Decimal]
    net_amber: Optional[Decimal]
    net_green: Optional[Decimal]
    net_total: Optional[Decimal]
    price_red: Optional[Decimal]
    price_amber: Optional[Decimal]
    price_green: Optional[Decimal]
    printed_fee_red: Optional[Decimal]
    printed_fee_amber: Optional[Decimal]
    printed_fee_green: Optional[Decimal]
    fee: WithBdp
    total_reported_tonnage_2a: Optional[Decimal]  # Section 2a's own "Total Tonnage" (should equal raw_total_tonnage)
    price_per_tonne_2a: Optional[Decimal]
    fee_2a: WithBdp


@dataclass
class ProducerRow:
    producer_id: str
    subsidiary_id: str
    name: str
    level: str
    by_material: dict[str, MaterialFigures]
    section1_total: WithBdp
    section2a: WithBdp
    section2b: WithBdp
    section2c: WithBdp
    section3: WithBdp
    section4: WithBdp
    section5: WithBdp
    total_bill: WithBdp
    pct_tonnage_vs_all_producers: Optional[Decimal]  # "Percentage of Producer Tonnage vs All Producers"
    pct_cost_vs_all_producers: Optional[Decimal]  # "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)"
    current_year_invoiced_total_to_date: Optional[Decimal]
    tonnage_change_advice: str  # "CHANGE" or ""
    liability_difference: Optional[Decimal]
    material_threshold_breached: str
    tonnage_threshold_breached: str
    pct_liability_difference: Optional[Decimal]
    material_pct_threshold_breached: str
    tonnage_pct_threshold_breached: str
    suggested_billing_instruction: str
    suggested_invoice_amount: Optional[Decimal]


def parse_producer_row(
    cells: list[str],
    offsets: dict[str, tuple[int, int]],
    materials_order: list[str],
    glass_name: Optional[str],
) -> ProducerRow:
    id_lo, id_hi = offsets["identity"]
    identity = cells[id_lo:id_hi]
    producer_id, subsidiary_id, name, _trading_name, level = identity[0:5]

    by_material: dict[str, MaterialFigures] = {}
    for material in materials_order:
        lo, hi = offsets[f"section1::{material}"]
        m = cells[lo:hi]
        is_glass = glass_name is not None and material == glass_name
        # Walk the block exactly as Section1MaterialsExporter wrote it.
        i = 1  # skip "Previous Invoiced Tonnage"
        i += 1 + len(RAG_KEYS)  # household
        i += 1 + len(RAG_KEYS)  # public bin
        if is_glass:
            i += 1 + len(RAG_KEYS)  # household drinks containers
        # Total-tonnage block: total(1), rag(6), grouped-rag(3)
        raw_total_tonnage = parse_decimal(m[i]); i += 1
        i += 6 + 3  # skip the RAG(6) and grouped-RAG(3) breakdown of that same block
        smcw_tonnage = parse_decimal(m[i]); i += 1
        i += 4  # actioned SMCW (total,red,amber,green)
        net_total = parse_decimal(m[i]); i += 1
        net_red = parse_decimal(m[i]); i += 1
        net_amber = parse_decimal(m[i]); i += 1
        net_green = parse_decimal(m[i]); i += 1
        i += 1  # residual SMCW
        i += 1  # tonnage change
        price_red = parse_money(m[i]); i += 1
        price_amber = parse_money(m[i]); i += 1
        price_green = parse_money(m[i]); i += 1
        fee_red = parse_money(m[i]); i += 1
        fee_amber = parse_money(m[i]); i += 1
        fee_green = parse_money(m[i]); i += 1
        fee, i = parse_with_bdp(m, i)

        lo2a, hi2a = offsets[f"section2a::{material}"]
        m2a = cells[lo2a:hi2a]
        j = 2  # skip Household Packaging Tonnage, Public Bin Tonnage
        if is_glass:
            j += 1  # skip Household Drinks Containers Tonnage
        total_reported_tonnage_2a = parse_decimal(m2a[j]); j += 1
        price_per_tonne_2a = parse_money(m2a[j]); j += 1
        fee_2a, j = parse_with_bdp(m2a, j)

        by_material[material] = MaterialFigures(
            raw_total_tonnage=raw_total_tonnage, smcw_tonnage=smcw_tonnage,
            net_red=net_red, net_amber=net_amber, net_green=net_green, net_total=net_total,
            price_red=price_red, price_amber=price_amber, price_green=price_green,
            printed_fee_red=fee_red, printed_fee_amber=fee_amber, printed_fee_green=fee_green,
            fee=fee,
            total_reported_tonnage_2a=total_reported_tonnage_2a,
            price_per_tonne_2a=price_per_tonne_2a,
            fee_2a=fee_2a,
        )

    s1_lo, s1_hi = offsets["section1_total"]
    s1 = cells[s1_lo:s1_hi]
    section1_total, _i = parse_with_bdp(s1, 0)
    tonnage_change_advice = s1[8].strip()

    def read_with_bdp(block_name: str) -> WithBdp:
        lo, hi = offsets[block_name]
        wb, _ = parse_with_bdp(cells[lo:hi], 0)
        return wb

    section2a = read_with_bdp("section2a_total_a")
    section2b = read_with_bdp("section2b_total")
    section2c = read_with_bdp("section2c_total")
    section3 = read_with_bdp("section3_total")
    section4 = read_with_bdp("section4_total")
    section5 = read_with_bdp("section5_total")
    total_bill = read_with_bdp("total_bill")

    pct_lo, pct_hi = offsets["pct_vs_all_producers"]
    pct_tonnage_vs_all_producers = parse_percent(cells[pct_lo])

    op_lo, op_hi = offsets["oneplus_2a2b2c"]
    op = cells[op_lo:op_hi]
    pct_cost_vs_all_producers = parse_percent(op[1])

    bi_lo, bi_hi = offsets["billing_instructions"]
    bi = cells[bi_lo:bi_hi]

    return ProducerRow(
        producer_id=producer_id.strip(),
        subsidiary_id=subsidiary_id.strip(),
        name=name.strip(),
        level=level.strip(),
        by_material=by_material,
        section1_total=section1_total,
        section2a=section2a,
        section2b=section2b,
        section2c=section2c,
        section3=section3,
        section4=section4,
        section5=section5,
        total_bill=total_bill,
        pct_tonnage_vs_all_producers=pct_tonnage_vs_all_producers,
        pct_cost_vs_all_producers=pct_cost_vs_all_producers,
        current_year_invoiced_total_to_date=parse_money(bi[0]),
        tonnage_change_advice=tonnage_change_advice,
        liability_difference=parse_money(bi[2]),
        material_threshold_breached=bi[3].strip().lstrip("‎"),
        tonnage_threshold_breached=bi[4].strip().lstrip("‎"),
        pct_liability_difference=parse_percent(bi[5]),
        material_pct_threshold_breached=bi[6].strip().lstrip("‎"),
        tonnage_pct_threshold_breached=bi[7].strip().lstrip("‎"),
        suggested_billing_instruction=bi[8].strip(),
        suggested_invoice_amount=parse_money(bi[9]),
    )


def find_producer_table(rows: list[list[str]]) -> tuple[int, int]:
    """Returns (header_row_index, first_data_row_index)."""
    header_row = find_row_containing(rows, "Registration Status Code")
    return header_row, header_row + 1


def read_producer_rows(rows: list[list[str]], first_data_row: int) -> list[list[str]]:
    out = []
    r = first_data_row
    while r < len(rows) and any(cell.strip() for cell in rows[r]):
        out.append(rows[r])
        r += 1
    return out


# ---------------------------------------------------------------------------
# Verification
# ---------------------------------------------------------------------------


@dataclass
class Discrepancy:
    producer: str
    section: str
    field: str
    expected: object
    actual: object


@dataclass
class VerificationResult:
    discrepancies: list[Discrepancy] = field(default_factory=list)

    def add(self, producer: str, section: str, field_name: str, expected, actual):
        self.discrepancies.append(Discrepancy(producer, section, field_name, expected, actual))


def approx_equal(a: Optional[Decimal], b: Optional[Decimal], tolerance: Decimal) -> bool:
    if a is None and b is None:
        return True
    if a is None or b is None:
        return False
    return abs(a - b) <= tolerance


def verify_modulation(mod: ModulationCalculation, materials_order: list[str], tol: Decimal, result: VerificationResult):
    """Check 1: the run-wide Red/Green modulation pricing is internally consistent."""
    section = "Modulation Calculation"

    sum_red_at_amber = sum((mod.by_material[m].total_red_at_amber_cost for m in materials_order), Decimal(0))
    sum_green_at_amber = sum((mod.by_material[m].total_green_at_amber_cost for m in materials_order), Decimal(0))

    if not approx_equal(sum_red_at_amber, mod.total_red_at_amber_cost, tol):
        result.add("(run-wide)", section, "Total row: Total Red Material at Amber Disposal Cost",
                    sum_red_at_amber, mod.total_red_at_amber_cost)
    if not approx_equal(sum_green_at_amber, mod.total_green_at_amber_cost, tol):
        result.add("(run-wide)", section, "Total row: Total Green Material at Amber Disposal Cost",
                    sum_green_at_amber, mod.total_green_at_amber_cost)

    if mod.total_green_at_amber_cost == 0:
        green_factor = Decimal(0)
    else:
        green_discount = (mod.red_factor - 1) * mod.total_red_at_amber_cost / mod.total_green_at_amber_cost
        green_factor = round_half_away_from_zero(1 - green_discount, 6)

    if not approx_equal(green_factor, mod.green_factor_printed, Decimal("0.000001")):
        result.add("(run-wide)", section, "Green Modulation Factor", green_factor, mod.green_factor_printed)

    for material in materials_order:
        mm = mod.by_material[material]

        expected_red_at_amber = round_half_away_from_zero(mm.red_net_tonnage * mm.amber_price, 2)
        expected_green_at_amber = round_half_away_from_zero(mm.green_net_tonnage * mm.amber_price, 2)
        if not approx_equal(expected_red_at_amber, mm.total_red_at_amber_cost, tol):
            result.add(f"(modulation:{material})", section, "Total Red Material at Amber Disposal Cost",
                        expected_red_at_amber, mm.total_red_at_amber_cost)
        if not approx_equal(expected_green_at_amber, mm.total_green_at_amber_cost, tol):
            result.add(f"(modulation:{material})", section, "Total Green Material at Amber Disposal Cost",
                        expected_green_at_amber, mm.total_green_at_amber_cost)

        expected_red_price = round_half_away_from_zero(mm.amber_price * mod.red_factor, 4)
        expected_green_price = round_half_away_from_zero(mm.amber_price * green_factor, 4)
        if not approx_equal(expected_red_price, mm.red_price, Decimal("0.0001")):
            result.add(f"(modulation:{material})", section, "Red Material Disposal Cost", expected_red_price, mm.red_price)
        if not approx_equal(expected_green_price, mm.green_price, Decimal("0.0001")):
            result.add(f"(modulation:{material})", section, "Green Material Disposal Cost", expected_green_price, mm.green_price)


def verify_modulation_vs_producers(
    mod: ModulationCalculation,
    late_reporting: LateReportingTonnages,
    materials_order: list[str],
    net_tonnage_sum_by_material: dict[str, RagAmounts],
    tonnage_tol: Decimal,
    result: VerificationResult,
):
    """
    Check 1b: the "Net Tonnage + Late Reporting Tonnage" figures that the Modulation
    Calculation section's pricing is built on should equal the sum of every L1
    producer's own Net Tonnage (by RAG group) plus the Late Reporting Tonnage
    parameter. This ties the run-wide modulation pricing back to the same
    per-producer data used in check 2, rather than trusting it as a disconnected
    input.
    """
    section = "Modulation Calculation vs producer data"
    for material in materials_order:
        mm = mod.by_material[material]
        lrt = late_reporting.by_material[material]
        summed = net_tonnage_sum_by_material[material]

        expected_red = summed.red + lrt.red
        expected_amber = summed.amber + lrt.amber
        expected_green = summed.green + lrt.green

        if not approx_equal(expected_red, mm.red_net_tonnage, tonnage_tol):
            result.add(f"(modulation:{material})", section,
                        "Red + Red Medical Net Tonnage + Late Reporting Tonnage", expected_red, mm.red_net_tonnage)
        if not approx_equal(expected_amber, mm.amber_net_tonnage, tonnage_tol):
            result.add(f"(modulation:{material})", section,
                        "Amber + Amber Medical Net Tonnage + Late Reporting Tonnage", expected_amber, mm.amber_net_tonnage)
        if not approx_equal(expected_green, mm.green_net_tonnage, tonnage_tol):
            result.add(f"(modulation:{material})", section,
                        "Green + Green Medical Net Tonnage + Late Reporting Tonnage", expected_green, mm.green_net_tonnage)


def producer_label(p: ProducerRow) -> str:
    label = f"Producer {p.producer_id}"
    if p.subsidiary_id:
        label += f" / Subsidiary {p.subsidiary_id}"
    if p.name:
        label += f" ({p.name})"
    return label


def verify_section1_disposal_fee(
    p: ProducerRow,
    materials_order: list[str],
    country_apportionment_pct: ByCountry,
    bad_debt_pct: Decimal,
    tol: Decimal,
    result: VerificationResult,
):
    """Checks 2: per-material and total Section 1 (LA Disposal Fee)."""
    label = producer_label(p)
    total_without_bdp = Decimal(0)
    total_bdp = Decimal(0)
    total_with_bdp = Decimal(0)
    total_by_country = ByCountry(Decimal(0), Decimal(0), Decimal(0), Decimal(0))

    for material in materials_order:
        mf = p.by_material[material]
        if mf.net_red is None:
            # No modulation data for this producer/material combination -- nothing to check.
            continue

        # The app zeroes a material's disposal fee outright if self-managed consumer
        # waste tonnage exceeds raw reported tonnage. For a multi-entity producer this
        # compares the *group's* SMCW tonnage against the *group's* summed raw tonnage
        # -- but the app prints exactly those two (already-aggregated) group figures as
        # this L1 row's own "Self Managed Consumer Waste Tonnage" and "Total Tonnage"
        # columns (see ProducerRowBuilder.GetL1TotalRow), so no L2 data is needed to
        # replicate it: this row's own printed columns are already the right inputs,
        # for single-entity and multi-entity producers alike.
        zero_override = (
            mf.smcw_tonnage is not None
            and mf.raw_total_tonnage is not None
            and mf.smcw_tonnage > mf.raw_total_tonnage
        )

        if zero_override:
            expected_fee_red = expected_fee_amber = expected_fee_green = Decimal(0)
        else:
            expected_fee_red = mf.net_red * mf.price_red
            expected_fee_amber = mf.net_amber * mf.price_amber
            expected_fee_green = mf.net_green * mf.price_green
        expected_fee_total = expected_fee_red + expected_fee_amber + expected_fee_green

        if not approx_equal(expected_fee_red, mf.printed_fee_red, tol):
            result.add(label, f"Section 1 :: {material}", "Producer Red Material Disposal Cost",
                        expected_fee_red, mf.printed_fee_red)
        if not approx_equal(expected_fee_amber, mf.printed_fee_amber, tol):
            result.add(label, f"Section 1 :: {material}", "Producer Amber Material Disposal Cost",
                        expected_fee_amber, mf.printed_fee_amber)
        if not approx_equal(expected_fee_green, mf.printed_fee_green, tol):
            result.add(label, f"Section 1 :: {material}", "Producer Green Material Disposal Cost",
                        expected_fee_green, mf.printed_fee_green)
        if not approx_equal(expected_fee_total, mf.fee.without_bdp, tol):
            result.add(label, f"Section 1 :: {material}", "Producer Disposal Fee w/o Bad Debt Provision",
                        expected_fee_total, mf.fee.without_bdp)

        expected_bdp = expected_fee_total * bad_debt_pct / 100
        if not approx_equal(expected_bdp, mf.fee.bdp, tol):
            result.add(label, f"Section 1 :: {material}", "Bad Debt Provision",
                        expected_bdp, mf.fee.bdp)

        expected_with_bdp_total = expected_fee_total * (1 + bad_debt_pct / 100)
        if not approx_equal(expected_with_bdp_total, mf.fee.total, tol):
            result.add(label, f"Section 1 :: {material}", "Producer Disposal Fee with Bad Debt Provision",
                        expected_with_bdp_total, mf.fee.total)

        expected_country = ByCountry(
            england=expected_with_bdp_total * country_apportionment_pct.england / 100,
            wales=expected_with_bdp_total * country_apportionment_pct.wales / 100,
            scotland=expected_with_bdp_total * country_apportionment_pct.scotland / 100,
            northern_ireland=expected_with_bdp_total * country_apportionment_pct.northern_ireland / 100,
        )
        for country_name, exp_val, actual_val in [
            ("England", expected_country.england, mf.fee.by_country.england),
            ("Wales", expected_country.wales, mf.fee.by_country.wales),
            ("Scotland", expected_country.scotland, mf.fee.by_country.scotland),
            ("Northern Ireland", expected_country.northern_ireland, mf.fee.by_country.northern_ireland),
        ]:
            if not approx_equal(exp_val, actual_val, tol):
                result.add(label, f"Section 1 :: {material}", f"{country_name} with Bad Debt Provision", exp_val, actual_val)

        total_without_bdp += expected_fee_total
        total_bdp += expected_bdp
        total_with_bdp += expected_with_bdp_total
        total_by_country = total_by_country + expected_country

    if not approx_equal(total_without_bdp, p.section1_total.without_bdp, tol):
        result.add(label, "Section 1 total", "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision",
                    total_without_bdp, p.section1_total.without_bdp)
    if not approx_equal(total_bdp, p.section1_total.bdp, tol):
        result.add(label, "Section 1 total", "Bad Debt Provision", total_bdp, p.section1_total.bdp)
    if not approx_equal(total_with_bdp, p.section1_total.total, tol):
        result.add(label, "Section 1 total", "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision",
                    total_with_bdp, p.section1_total.total)


def verify_section2a(
    p: ProducerRow,
    materials_order: list[str],
    comms: CommsCostParameters,
    bad_debt_pct: Decimal,
    tol: Decimal,
    result: VerificationResult,
):
    """Check 3a: Comms Costs by Material -- tonnage x price per material, apportioned by 1+4%."""
    label = producer_label(p)
    apportionment = comms.one_plus_four_apportionment_pct
    total_without_bdp = Decimal(0)
    total_bdp = Decimal(0)
    total_with_bdp = Decimal(0)

    for material in materials_order:
        mf = p.by_material[material]
        if mf.total_reported_tonnage_2a is None:
            continue

        # The same raw (pre-SMCW) tonnage feeds both Section 1's disposal fee and
        # Section 2a's comms fee -- cross-check the file prints the same number twice.
        if mf.raw_total_tonnage is not None and not approx_equal(mf.raw_total_tonnage, mf.total_reported_tonnage_2a, Decimal("0.001")):
            result.add(label, f"Section 2a :: {material}", "Total Tonnage (vs Section 1's own copy)",
                        mf.raw_total_tonnage, mf.total_reported_tonnage_2a)

        expected_price = comms.price_per_tonne_by_material[material]
        if mf.price_per_tonne_2a is not None and not approx_equal(expected_price, mf.price_per_tonne_2a, Decimal("0.0001")):
            result.add(label, f"Section 2a :: {material}", "Price per Tonne", expected_price, mf.price_per_tonne_2a)

        expected_fee_without_bdp = mf.total_reported_tonnage_2a * expected_price
        if not approx_equal(expected_fee_without_bdp, mf.fee_2a.without_bdp, tol):
            result.add(label, f"Section 2a :: {material}", "Producer Total Cost w/o Bad Debt Provision",
                        expected_fee_without_bdp, mf.fee_2a.without_bdp)

        expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
        if not approx_equal(expected_bdp, mf.fee_2a.bdp, tol):
            result.add(label, f"Section 2a :: {material}", "Bad Debt Provision", expected_bdp, mf.fee_2a.bdp)

        expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
        if not approx_equal(expected_with_bdp_total, mf.fee_2a.total, tol):
            result.add(label, f"Section 2a :: {material}", "Producer Total Cost with Bad Debt Provision",
                        expected_with_bdp_total, mf.fee_2a.total)

        expected_country = ByCountry(
            england=expected_with_bdp_total * apportionment.england / 100,
            wales=expected_with_bdp_total * apportionment.wales / 100,
            scotland=expected_with_bdp_total * apportionment.scotland / 100,
            northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
        )
        for country_name, exp_val, actual_val in [
            ("England", expected_country.england, mf.fee_2a.by_country.england),
            ("Wales", expected_country.wales, mf.fee_2a.by_country.wales),
            ("Scotland", expected_country.scotland, mf.fee_2a.by_country.scotland),
            ("Northern Ireland", expected_country.northern_ireland, mf.fee_2a.by_country.northern_ireland),
        ]:
            if not approx_equal(exp_val, actual_val, tol):
                result.add(label, f"Section 2a :: {material}", f"{country_name} with Bad Debt Provision", exp_val, actual_val)

        total_without_bdp += expected_fee_without_bdp
        total_bdp += expected_bdp
        total_with_bdp += expected_with_bdp_total

    if not approx_equal(total_without_bdp, p.section2a.without_bdp, tol):
        result.add(label, "Section 2a total", "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision",
                    total_without_bdp, p.section2a.without_bdp)
    if not approx_equal(total_bdp, p.section2a.bdp, tol):
        result.add(label, "Section 2a total", "Total Bad Debt Provision", total_bdp, p.section2a.bdp)
    if not approx_equal(total_with_bdp, p.section2a.total, tol):
        result.add(label, "Section 2a total", "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision",
                    total_with_bdp, p.section2a.total)


def verify_section2b(p: ProducerRow, comms: CommsCostParameters, bad_debt_pct: Decimal, tol: Decimal, result: VerificationResult):
    """Check 3b: Comms Costs UK-wide, apportioned to this producer by its share of all producers' tonnage."""
    label = producer_label(p)
    if p.pct_tonnage_vs_all_producers is None:
        return
    apportionment = comms.one_plus_four_apportionment_pct

    expected_fee_without_bdp = comms.uk_wide.total * p.pct_tonnage_vs_all_producers / 100
    if not approx_equal(expected_fee_without_bdp, p.section2b.without_bdp, tol):
        result.add(label, "Section 2b", "2b Total Producer Fee for Comms Costs - UK wide w/o Bad Debt provision",
                    expected_fee_without_bdp, p.section2b.without_bdp)

    expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
    if not approx_equal(expected_bdp, p.section2b.bdp, tol):
        result.add(label, "Section 2b", "Bad Debt Provision for 2b", expected_bdp, p.section2b.bdp)

    expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
    if not approx_equal(expected_with_bdp_total, p.section2b.total, tol):
        result.add(label, "Section 2b", "2b Total Producer Fee for Comms Costs - UK wide with Bad Debt provision",
                    expected_with_bdp_total, p.section2b.total)

    expected_country = ByCountry(
        england=expected_with_bdp_total * apportionment.england / 100,
        wales=expected_with_bdp_total * apportionment.wales / 100,
        scotland=expected_with_bdp_total * apportionment.scotland / 100,
        northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
    )
    for country_name, exp_val, actual_val in [
        ("England", expected_country.england, p.section2b.by_country.england),
        ("Wales", expected_country.wales, p.section2b.by_country.wales),
        ("Scotland", expected_country.scotland, p.section2b.by_country.scotland),
        ("Northern Ireland", expected_country.northern_ireland, p.section2b.by_country.northern_ireland),
    ]:
        if not approx_equal(exp_val, actual_val, tol):
            result.add(label, "Section 2b", f"{country_name} Total with Bad Debt provision", exp_val, actual_val)


def verify_section2c(p: ProducerRow, comms: CommsCostParameters, bad_debt_pct: Decimal, tol: Decimal, result: VerificationResult):
    """
    Check 3c: Comms Costs by Country, apportioned to this producer by its share of all
    producers' tonnage. Unlike 2b, the per-country split here follows the *raw* by-country
    comms cost split directly (not the 1+4 apportionment).
    """
    label = producer_label(p)
    if p.pct_tonnage_vs_all_producers is None:
        return
    pct = p.pct_tonnage_vs_all_producers / 100

    expected_fee_without_bdp = comms.by_country.total * pct
    if not approx_equal(expected_fee_without_bdp, p.section2c.without_bdp, tol):
        result.add(label, "Section 2c", "2c Total Producer Fee for Comms Costs - by Country w/o Bad Debt provision",
                    expected_fee_without_bdp, p.section2c.without_bdp)

    expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
    if not approx_equal(expected_bdp, p.section2c.bdp, tol):
        result.add(label, "Section 2c", "Bad Debt Provision for 2c", expected_bdp, p.section2c.bdp)

    expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
    if not approx_equal(expected_with_bdp_total, p.section2c.total, tol):
        result.add(label, "Section 2c", "2c Total Producer Fee for Comms Costs - by Country with Bad Debt provision",
                    expected_with_bdp_total, p.section2c.total)

    expected_country = ByCountry(
        england=comms.by_country.england * (1 + bad_debt_pct / 100) * pct,
        wales=comms.by_country.wales * (1 + bad_debt_pct / 100) * pct,
        scotland=comms.by_country.scotland * (1 + bad_debt_pct / 100) * pct,
        northern_ireland=comms.by_country.northern_ireland * (1 + bad_debt_pct / 100) * pct,
    )
    for country_name, exp_val, actual_val in [
        ("England", expected_country.england, p.section2c.by_country.england),
        ("Wales", expected_country.wales, p.section2c.by_country.wales),
        ("Scotland", expected_country.scotland, p.section2c.by_country.scotland),
        ("Northern Ireland", expected_country.northern_ireland, p.section2c.by_country.northern_ireland),
    ]:
        if not approx_equal(exp_val, actual_val, tol):
            result.add(label, "Section 2c", f"{country_name} Total with Bad Debt provision", exp_val, actual_val)


def verify_producer_pct_cost_vs_all_producers(
    p: ProducerRow, header_total_1_2a2b2c: Decimal, result: VerificationResult
):
    """
    Checks that "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)" -- the input
    that sections 3, 4 and 5 apportion by -- is this producer's own (1+2a+2b+2c) total as a
    percentage of the run-wide (1+2a+2b+2c) total (read once from the file's header row).
    """
    label = producer_label(p)
    producer_total = p.section1_total.total + p.section2a.total + p.section2b.total + p.section2c.total

    if header_total_1_2a2b2c == 0:
        expected_pct = Decimal(0)
    else:
        expected_pct = producer_total / header_total_1_2a2b2c * 100

    if not approx_equal(expected_pct, p.pct_cost_vs_all_producers, Decimal("0.01")):
        result.add(label, "Producer Percentage of Overall Producer Cost",
                    "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)",
                    expected_pct, p.pct_cost_vs_all_producers)


def verify_sections_3_4_5(
    p: ProducerRow,
    comms: CommsCostParameters,
    other_params: OtherParameters,
    bad_debt_pct: Decimal,
    tol: Decimal,
    result: VerificationResult,
):
    """
    Check 4: SA Operating Costs, LA Data Prep Costs, SA Set Up Costs. All three are
    structurally identical -- a fixed run-wide total apportioned to this producer by its
    'Percentage of Overall Producer Cost for (1+2a+2b+2c)', then split by country.
    Section 4 uses its own ("4 Country Apportionment %s") apportionment; 3 and 5 use 1+4%.
    """
    label = producer_label(p)
    if p.pct_cost_vs_all_producers is None:
        return
    producer_pct = p.pct_cost_vs_all_producers

    sections = [
        ("Section 3", other_params.sa_operating_cost_total, comms.one_plus_four_apportionment_pct, p.section3),
        ("Section 4", other_params.la_data_prep_charge_total, other_params.la_data_prep_apportionment_pct, p.section4),
        ("Section 5", other_params.scheme_setup_cost_total, comms.one_plus_four_apportionment_pct, p.section5),
    ]

    for section_name, section_total, apportionment, printed in sections:
        expected_without_bdp = producer_pct * section_total / 100
        if not approx_equal(expected_without_bdp, printed.without_bdp, tol):
            result.add(label, section_name, "w/o Bad Debt Provision", expected_without_bdp, printed.without_bdp)

        expected_bdp = expected_without_bdp * bad_debt_pct / 100
        if not approx_equal(expected_bdp, printed.bdp, tol):
            result.add(label, section_name, "Bad Debt Provision", expected_bdp, printed.bdp)

        expected_with_bdp_total = expected_without_bdp * (1 + bad_debt_pct / 100)
        if not approx_equal(expected_with_bdp_total, printed.total, tol):
            result.add(label, section_name, "with Bad Debt Provision", expected_with_bdp_total, printed.total)

        expected_country = ByCountry(
            england=expected_with_bdp_total * apportionment.england / 100,
            wales=expected_with_bdp_total * apportionment.wales / 100,
            scotland=expected_with_bdp_total * apportionment.scotland / 100,
            northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
        )
        for country_name, exp_val, actual_val in [
            ("England", expected_country.england, printed.by_country.england),
            ("Wales", expected_country.wales, printed.by_country.wales),
            ("Scotland", expected_country.scotland, printed.by_country.scotland),
            ("Northern Ireland", expected_country.northern_ireland, printed.by_country.northern_ireland),
        ]:
            if not approx_equal(exp_val, actual_val, tol):
                result.add(label, section_name, f"{country_name} Total with Bad Debt provision", exp_val, actual_val)


def verify_total_bill(p: ProducerRow, tol: Decimal, result: VerificationResult):
    """
    Check 5: Total Producer Bill = Section 1 + 2a + 2b + 2c + 3 + 4 + 5.

    Sections 1 through 5 are each already independently verified above --
    this check only re-derives the *addition*, using every
    section's own printed total. That's why the tolerance here is a little
    looser than elsewhere: each of the 7 inputs was itself rounded once for
    display, so their sum can legitimately differ from the printed grand
    total (which was rounded once from the true, unrounded sum) by a few
    pence -- that is expected rounding noise, not a discrepancy.
    """
    label = producer_label(p)
    loose_tol = max(tol, Decimal("0.05"))

    expected_total = (
        p.section1_total.total
        + p.section2a.total
        + p.section2b.total
        + p.section2c.total
        + p.section3.total
        + p.section4.total
        + p.section5.total
    )
    if not approx_equal(expected_total, p.total_bill.total, loose_tol):
        result.add(label, "Total Producer Bill", "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision",
                    expected_total, p.total_bill.total)

    expected_by_country = (
        p.section1_total.by_country
        + p.section2a.by_country
        + p.section2b.by_country
        + p.section2c.by_country
        + p.section3.by_country
        + p.section4.by_country
        + p.section5.by_country
    )
    for country_name, exp_val, actual_val in [
        ("England", expected_by_country.england, p.total_bill.by_country.england),
        ("Wales", expected_by_country.wales, p.total_bill.by_country.wales),
        ("Scotland", expected_by_country.scotland, p.total_bill.by_country.scotland),
        ("Northern Ireland", expected_by_country.northern_ireland, p.total_bill.by_country.northern_ireland),
    ]:
        if not approx_equal(exp_val, actual_val, loose_tol):
            result.add(label, "Total Producer Bill", f"{country_name} Total with Bad Debt provision", exp_val, actual_val)


def verify_billing_instruction(p: ProducerRow, params: OtherParameters, tol: Decimal, result: VerificationResult):
    """Check 6: suggested billing instruction and invoice amount."""
    label = producer_label(p)
    prior = p.current_year_invoiced_total_to_date

    if prior is None:
        expected_liability_diff = None
    else:
        expected_liability_diff = (
            round_half_away_from_zero(p.total_bill.total, 2)
            - round_half_away_from_zero(prior, 2)
        )

    if not approx_equal(expected_liability_diff, p.liability_difference, tol):
        result.add(label, "Billing Instruction", "Liability Difference (Calc vs Prev)",
                    expected_liability_diff, p.liability_difference)

    def threshold_flag(diff: Optional[Decimal], increase: Decimal, decrease: Decimal) -> str:
        if diff is None:
            return HYPHEN
        if diff >= increase:
            return "+ve"
        if diff <= decrease:
            return "-ve"
        return HYPHEN

    expected_material_flag = (
        HYPHEN if prior is None or expected_liability_diff is None
        else threshold_flag(expected_liability_diff, params.materiality_increase.amount, params.materiality_decrease.amount)
    )
    if expected_material_flag != p.material_threshold_breached:
        result.add(label, "Billing Instruction", "Material £ Threshold Breached",
                    expected_material_flag, p.material_threshold_breached)

    tonnage_changed = p.tonnage_change_advice == "CHANGE"
    expected_tonnage_flag = (
        HYPHEN if prior is None or not tonnage_changed or expected_liability_diff is None
        else threshold_flag(expected_liability_diff, params.tonnage_change_increase.amount, params.tonnage_change_decrease.amount)
    )
    if expected_tonnage_flag != p.tonnage_threshold_breached:
        result.add(label, "Billing Instruction", "Tonnage £ Threshold Breached (if tonnage changed)",
                    expected_tonnage_flag, p.tonnage_threshold_breached)

    if prior is None or expected_liability_diff is None or prior == 0:
        expected_pct_diff = None
    else:
        expected_pct_diff = round_half_away_from_zero(expected_liability_diff / prior * 100, 2)
    if not approx_equal(expected_pct_diff, p.pct_liability_difference, Decimal("0.01")):
        result.add(label, "Billing Instruction", "% Liability Difference (Calc vs Prev)",
                    expected_pct_diff, p.pct_liability_difference)

    expected_material_pct_flag = (
        HYPHEN if prior is None
        else threshold_flag(expected_pct_diff, params.materiality_increase.percentage, params.materiality_decrease.percentage)
    )
    if expected_material_pct_flag != p.material_pct_threshold_breached:
        result.add(label, "Billing Instruction", "Material % Threshold Breached",
                    expected_material_pct_flag, p.material_pct_threshold_breached)

    expected_tonnage_pct_flag = (
        HYPHEN if prior is None or not tonnage_changed
        else threshold_flag(expected_pct_diff, params.tonnage_change_increase.percentage, params.tonnage_change_decrease.percentage)
    )
    if expected_tonnage_pct_flag != p.tonnage_pct_threshold_breached:
        result.add(label, "Billing Instruction", "Tonnage % Threshold Breached (if tonnage changed)",
                    expected_tonnage_pct_flag, p.tonnage_pct_threshold_breached)

    any_breached = any(
        flag != HYPHEN
        for flag in (expected_material_flag, expected_tonnage_flag, expected_material_pct_flag, expected_tonnage_pct_flag)
    )

    if prior is None:
        expected_instruction = "INITIAL"
    elif expected_liability_diff is not None and expected_liability_diff > 0 and any_breached:
        expected_instruction = "DELTA"
    elif expected_liability_diff is not None and expected_liability_diff < 0 and any_breached:
        expected_instruction = "REBILL"
    else:
        expected_instruction = HYPHEN

    if expected_instruction != p.suggested_billing_instruction:
        result.add(label, "Billing Instruction", "Suggested Billing Instruction",
                    expected_instruction, p.suggested_billing_instruction)

    if expected_instruction in ("INITIAL", "REBILL"):
        expected_amount = p.total_bill.total
    elif expected_instruction == "DELTA":
        expected_amount = expected_liability_diff
    else:
        expected_amount = None

    if not approx_equal(expected_amount, p.suggested_invoice_amount, tol):
        result.add(label, "Billing Instruction", "Suggested Invoice Amount",
                    expected_amount, p.suggested_invoice_amount)


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("results_csv", help="Path to a Results CSV export")
    parser.add_argument("--tolerance", type=str, default="0.01",
                         help="Currency comparison tolerance in pounds (default: 0.01)")
    parser.add_argument("--verbose", action="store_true", help="Print every discrepancy in full")
    args = parser.parse_args()

    tol = Decimal(args.tolerance)
    rows = load_rows(args.results_csv)

    lapcap = parse_lapcap_data(rows)
    la_disposal = parse_la_disposal_cost_data(rows, lapcap.materials_order)
    modulation = parse_modulation_calculation(rows, lapcap.materials_order)
    late_reporting = parse_late_reporting_tonnages(rows, lapcap.materials_order)
    other_params = parse_other_parameters(rows)
    comms = parse_comms_cost_parameters(rows, lapcap.materials_order)

    if modulation is None:
        print("This file has no 'Modulation Calculation' section -- it is a pre-modulation")
        print("Results file, which this version of the script does not support.")
        return 2

    # Sanity-check the modulation section's Amber price against LA Disposal Cost Data
    # (both should be the same underlying flat price-per-tonne).
    for material in lapcap.materials_order:
        expected_amber = la_disposal.price_per_tonne[material]
        actual_amber = modulation.by_material[material].amber_price
        if expected_amber is not None and actual_amber is not None and abs(expected_amber - actual_amber) > Decimal("0.0001"):
            print(f"WARNING: Amber price mismatch between 'LA Disposal Cost Data' and 'Modulation Calculation' "
                  f"for {material}: {expected_amber} vs {actual_amber}")

    glass_name = next((m for m in lapcap.materials_order if m.strip().lower() == "glass"), None)

    header_row, first_data_row = find_producer_table(rows)
    schema = build_producer_table_schema(lapcap.materials_order, glass_name)
    offsets = block_offsets(schema)

    # The "group header" row -- one row above the column-header row -- carries each
    # PartExporter's AppendGroupHeader output, written once per file at the same column
    # positions as the per-producer data rows. That's where the run-wide (1+2a+2b+2c)
    # total (the denominator for each producer's share of overall cost) is printed.
    op_lo, _op_hi = offsets["oneplus_2a2b2c"]
    header_total_1_2a2b2c = d(parse_money(rows[header_row - 1][op_lo]))

    raw_producer_rows = read_producer_rows(rows, first_data_row)
    id_lo, id_hi = offsets["identity"]

    def row_identity(cells: list[str]) -> tuple[str, str]:
        idn = cells[id_lo:id_hi]
        return idn[0].strip(), idn[4].strip()  # producer_id, level

    result = VerificationResult()
    verify_modulation(modulation, lapcap.materials_order, tol, result)

    net_tonnage_sum_by_material = {
        m: RagAmounts(red=Decimal(0), amber=Decimal(0), green=Decimal(0)) for m in lapcap.materials_order
    }

    l1_count = 0
    for cells in raw_producer_rows:
        producer_id, level = row_identity(cells)
        if level != "1" or producer_id == "":
            continue  # L2 subsidiary row, or the single overall-total row: out of scope

        p = parse_producer_row(cells, offsets, lapcap.materials_order, glass_name)
        l1_count += 1

        # Attach the run-wide modulation prices as each material's price, for
        # verify_section1_disposal_fee (per-producer prices are the same run-wide
        # value, printed again on every row by Section1MaterialsExporter -- we
        # cross-check the file's own per-row copies against the printed values below).
        for material in lapcap.materials_order:
            mf = p.by_material[material]
            mm = modulation.by_material[material]
            if mf.price_red is not None and abs(mf.price_red - mm.red_price) > Decimal("0.0001"):
                result.add(producer_label(p), f"Section 1 :: {material}", "Red + Red Medical Material Price per Tonne",
                            mm.red_price, mf.price_red)
            if mf.price_amber is not None and abs(mf.price_amber - mm.amber_price) > Decimal("0.0001"):
                result.add(producer_label(p), f"Section 1 :: {material}", "Amber + Amber Medical Material Price per Tonne",
                            mm.amber_price, mf.price_amber)
            if mf.price_green is not None and abs(mf.price_green - mm.green_price) > Decimal("0.0001"):
                result.add(producer_label(p), f"Section 1 :: {material}", "Green + Green Medical Material Price per Tonne",
                            mm.green_price, mf.price_green)

            if mf.net_red is not None:
                totals = net_tonnage_sum_by_material[material]
                net_tonnage_sum_by_material[material] = RagAmounts(
                    red=totals.red + mf.net_red, amber=totals.amber + mf.net_amber, green=totals.green + mf.net_green
                )

        verify_section1_disposal_fee(p, lapcap.materials_order, lapcap.country_apportionment_pct,
                                      other_params.bad_debt_pct, tol, result)
        verify_section2a(p, lapcap.materials_order, comms, other_params.bad_debt_pct, tol, result)
        verify_section2b(p, comms, other_params.bad_debt_pct, tol, result)
        verify_section2c(p, comms, other_params.bad_debt_pct, tol, result)
        verify_producer_pct_cost_vs_all_producers(p, header_total_1_2a2b2c, result)
        verify_sections_3_4_5(p, comms, other_params, other_params.bad_debt_pct, tol, result)
        verify_total_bill(p, tol, result)
        verify_billing_instruction(p, other_params, tol, result)

    verify_modulation_vs_producers(modulation, late_reporting, lapcap.materials_order,
                                    net_tonnage_sum_by_material, Decimal("0.01"), result)

    print(f"Checked {l1_count} Level-1 producer row(s) in {args.results_csv}")
    print(f"Materials: {', '.join(lapcap.materials_order)}")
    print(f"Red Modulation Factor: {modulation.red_factor}   Green Modulation Factor (printed): {modulation.green_factor_printed}")
    print()

    if not result.discrepancies:
        print("No discrepancies found.")
        return 0

    print(f"{len(result.discrepancies)} discrepancy(ies) found:\n")
    shown = result.discrepancies if args.verbose else result.discrepancies[:50]
    for disc in shown:
        print(f"  [{disc.producer}] {disc.section} :: {disc.field}")
        print(f"      expected: {disc.expected}")
        print(f"      actual:   {disc.actual}")
    if not args.verbose and len(result.discrepancies) > 50:
        print(f"  ... and {len(result.discrepancies) - 50} more (use --verbose to see all)")

    return 1


if __name__ == "__main__":
    sys.exit(main())
