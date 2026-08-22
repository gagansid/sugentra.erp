# Module Log: Finance & Accounting

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Finance is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- Finance's release-of-goods approval (verifying payment/export credit instrument before Sales releases a reservation) is a Sales/Finance concern — Finance does not call Inventory directly for this; it approves within Sales' workflow, and Sales is the one that then calls Inventory's `ReleaseReservationAsync`.
- Inventory's Phase 1 design deliberately tracks **quantity only**, not stock value/costing — Inventory's design doc (`docs/modules/inventory.md`) leans toward deferring HPP/weighted-average costing entirely to Finance. When Finance is built, it will need to read `Inventory_StockLedgers` (quantity movements) plus `Inventory_LandedCostAllocations` (cost components per batch) via a contract query to compute costing/HPP itself — Inventory will not calculate or expose a "current stock value" number in Phase 1.
- Scrap/damage write-off approval may require Finance sign-off per the original workflow narrative — if so, Finance's approval is consumed by Production/Inventory's write-off flow the same way Sales consumes Finance's release approval (approval lives in the initiating module's workflow, not as an Inventory dependency).
