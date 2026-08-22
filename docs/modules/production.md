# Module Log: Production

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Production is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- Work Order creation and its capacity/manpower approval gate are entirely internal to Production.
- Material consumption for a Work Order calls **Inventory's** `IStockConsumptionService.ConsumeForWorkOrderAsync(itemId, qty, warehouseId, workOrderReference)` — Inventory posts the `StockLedger(Consumption)` entry and decrements `StockBalance`; Production never touches Inventory's tables directly.
- Subcontractor/maklon transfers (material sent to a vendor location for partial processing, returned as semi-finished goods) are posted as `Inventory_StockMutations` with `MutationType = ToVendor`/`FromVendor` — Production triggers this via the same contract surface (exact method TBD, likely an extension of `IStockConsumptionService` or a new `ISubcontractTransferService`).
- Finished-goods receipt from a completed Work Order posts a `StockLedger(Receipt)` into the finished-goods warehouse — same contract pattern as Goods Receipt from Procurement.
- Scrap/damage write-off: Production (or Finance) approves the write-off, then Production calls Inventory to post `StockLedger(Adjustment)` — Inventory does not decide whether a write-off is approved, it only executes the resulting stock movement.
- Customer-return rework: Inventory places a returned batch into `Inventory_QuarantineHolds` (`HoldReason = CustomerReturn`); Production issues a new, smaller rework Work Order referencing that held batch instead of repeating a full production cycle. Production reads the hold reference via contract, does not query `Inventory_QuarantineHolds` directly.
