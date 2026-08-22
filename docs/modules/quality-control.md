# Module Log: Quality Control

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Quality Control is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- On goods receipt, Inventory creates the `Inventory_Batches` record and notifies QC that a batch is pending inspection (exact trigger mechanism TBD — likely QC polls/subscribes via a contract query rather than Inventory pushing, to keep Inventory decoupled from QC's inspection workflow).
- QC's inspection decision (Accept / Partial Accept / Reject) is QC's own business logic and approval gate — Inventory does not decide it, only reacts to the result by placing the batch into `Inventory_QuarantineHolds` (`HoldReason = QcFailed`) when not a clean accept, and updating `StockBalance` for the accepted quantity.
- QC calls an Inventory contract (e.g. `IQuarantineDecisionService.ApplyInspectionResultAsync(batchId, decision, acceptedQty)`) to communicate the outcome — QC never writes to `Inventory_QuarantineHolds`/`Inventory_StockBalances` directly.
- Production-side QC (pre-shipment inspection) similarly interacts with Inventory's outbound quarantine/fumigation hold before container loading, using the same pattern.
