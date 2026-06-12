# Billing JSON Schema Changes

This document describes the structural improvements made when moving from the original billing JSON format to the new schema used from the 2026 billing year onwards.

## Summary

The new schema eliminates redundancy, simplifies per-producer data shapes, and removes currency symbols from monetary values, making the output easier to parse programmatically.

---

## Root-level additions

The new schema promotes three fields to the document root that were previously nested inside `calcResultDetail`:

| Field | Type | Example |
|---|---|---|
| `runId` | integer | `42` |
| `financialYear` | string | `"2025-26"` |
| `badDebtProvisionPercentage` | string | `"6.00"` |

These fields allow a consumer to identify and filter documents without having to descend into nested objects.

---

## `calculationResults` replaced by `producers` and `materials`

### Old structure

```json
{
  "calculationResults": {
    "producerCalculationResults": [
      {
        "producerID": "1",
        "producerDisposalFeesWithBadDebtProvision1": {
          "materialBreakdown": [
            {
              "materialName": "Aluminium",
              "pricePerTonne": "£0.5000",
              "producerDisposalFeeWithBadDebtProvision": "£643.98",
              ...
            }
          ]
        },
        "feeForLADisposalCosts1": {
          "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision": "£607.52",
          "badDebtProvisionForLADisposalCosts": "£36.45"
        },
        "feeForCommsCostsWithBadDebtProvision_2a": { ... },
        "feeForCommsCostsWithBadDebtProvision_2b": { ... },
        "feeForCommsCostsWithBadDebtProvision_2c": { ... },
        "feeForSAOperatingCostsWithBadDebtProvision_3": { ... },
        "feeForLADataPrepCostsWithBadDebtProvision_4": { ... },
        "feeForSASetUpCostsWithBadDebtProvision_5": { ... }
      }
    ]
  }
}
```

### New structure

```json
{
  "materials": [
    {
      "materialName": "Aluminium",
      "disposalPricePerTonne": {
        "redAndRedMedical":     "0.5000",
        "amberAndAmberMedical": "0.4500",
        "greenAndGreenMedical": "0.3000"
      },
      "commsPricePerTonne": "0.4200"
    }
  ],
  "producers": [
    {
      "producerID": "1",
      "disposalCosts":        { "base": "607.52", "badDebtProvision": "36.45", "total": "643.97", "england": "...", ... },
      "commsCostsByMaterial": { "base": "...", "badDebtProvision": "...", "total": "...", "england": "...", ... },
      "commsCostsUKWide":     { ... },
      "commsCostsByCountry":  { ... },
      "saOperatingCosts":     { ... },
      "laDataPrepCosts":      { ... },
      "saSetUpCosts":         { ... },
      "totalBill":            { ... },
      "disposalFeesByMaterial": [ ... ]
    }
  ]
}
```

---

## Key improvements

### 1. Price-per-tonne moved to run level

**Before:** each material entry in every producer's `materialBreakdown` repeated `pricePerTonne`. For a run with 500 producers and 8 materials, this was 4,000 repetitions of the same value.

**After:** `materials[]` at the root contains one entry per material with the disposal and comms prices for the whole run. Consumers join on `materialName`.

### 2. Consistent fee shape across all sections

**Before:** each of the seven fee sections (1, 2a, 2b, 2c, 3, 4, 5) had its own bespoke property names:

```json
"feeForLADisposalCosts1": {
  "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision": "£607.52",
  "badDebtProvisionForLADisposalCosts": "£36.45"
}
```

**After:** all sections use the same compact `feeWithCountries` shape:

```json
"disposalCosts": {
  "base":             "607.52",
  "badDebtProvision": "36.45",
  "total":            "643.97",
  "england":          "321.98",
  "wales":            "160.99",
  "scotland":         "96.00",
  "northernIreland":  "65.00"
}
```

This means consumers can handle every fee section with the same parsing logic. Section names are also shortened and consistent (`disposalCosts`, `commsCostsByMaterial`, `commsCostsUKWide`, `commsCostsByCountry`, `saOperatingCosts`, `laDataPrepCosts`, `saSetUpCosts`).

### 3. Currency values without `£` symbols

**Before:** all monetary values carried a `£` prefix (e.g. `"£643.98"`), requiring callers to strip the symbol before parsing.

**After:** all monetary values are plain decimal strings (e.g. `"643.98"`), matching standard JSON numeric conventions and eliminating the parsing step.

The pattern for 2 decimal place values is now `^-?[0-9]+\.[0-9]{2}$`; for 4 decimal place prices it is `^[0-9]+\.[0-9]{4}$`.

### 4. RAG modulation expressed at the right level

**Before:** per-producer material items repeated RAG-split prices in each producer's `materialBreakdown`.

**After:** the run-level `materials[].disposalPricePerTonne` carries the RAG breakdown (`redAndRedMedical`, `amberAndAmberMedical`, `greenAndGreenMedical`) once. Per-producer tonnage items still carry RAG-split tonnage and fee breakdowns where the modulated path is active.

### 5. Shorter property names

| Old name | New name |
|---|---|
| `feeForLADisposalCosts1` | `disposalCosts` |
| `feeForCommsCostsWithBadDebtProvision_2a` | `commsCostsByMaterial` |
| `feeForCommsCostsWithBadDebtProvision_2b` | `commsCostsUKWide` |
| `feeForCommsCostsWithBadDebtProvision_2c` | `commsCostsByCountry` |
| `feeForSAOperatingCostsWithBadDebtProvision_3` | `saOperatingCosts` |
| `feeForLADataPrepCostsWithBadDebtProvision_4` | `laDataPrepCosts` |
| `feeForSASetUpCostsWithBadDebtProvision_5` | `saSetUpCosts` |
| `producerDisposalFeeWithBadDebtProvision` | `fee` |
| `producerDisposalFeeWithoutBadDebtProvision` | `feeWithoutBadDebt` |
| `calculationOfSuggestedBillingInstructionsAndInvoiceAmounts` | `invoice` |

---

## Schema files

| File | Purpose |
|---|---|
| `2025-billing.schema.json` | Validates billing JSON for 2025 runs (non-modulated) |
| `2026-billing.schema.json` | Validates billing JSON for 2026 runs (with RAG modulation) |
| `2026-billing-proposal.schema.json` | Original design proposal that informed the 2026 schema |
