# Module Log: Procurement

> Purchase Requisition (PR) and Purchase Order (PO) are implemented end-to-end (API + UI). Vendor Bill/AP and the Goods Receipt↔PO link are still not implemented — see "Deviations from the original plan" below. Original v1 scope kept as-is for history; session log follows it.

## v1 Scope (decided)
- **Purchase Requisition (PR)** — header + lines, optional (not a hard prerequisite for PO). Workflow: Draft → WaitingApproval → Approved/Rejected, via the same `IApprovalService` + `Setting_ApprovalMatrices` pattern already used by Inventory's StockMutation/StockOpname.
- **Purchase Order (PO)** — header + lines. Can be created either from an Approved PR, or directly (PR optional). Same approval workflow as PR.
- **Vendor Bill (AP)** — **out of scope for v1**, deferred until the Finance module exists (AP/GL posting is Finance's concern, not Procurement's).
- **Goods Receipt link** — `Inventory_GoodsReceipts` gets a nullable `PurchaseOrderId` FK (additive migration, owned/applied from Procurement's script folder or a small Inventory follow-up script — TBD at implementation time). When a GR referencing a PO is posted, Procurement's PO status is updated to reflect partial/fully received (exact mechanism — event/contract callback vs. Procurement polling — to be finalized at implementation time; likely `IGoodsReceiptStockService`-adjacent contract call from Inventory back into Procurement, or Procurement's `PurchaseOrderUseCase` exposing an `IPurchaseOrderReceiptService` in `Shared/Contracts/` that Inventory calls after posting a GR with a non-null `PurchaseOrderId`).
- Reuses existing infra: `MasterData_BusinessPartners` (`PartnerType = Supplier/Both`) for vendor master, `MasterData_PriceListHeaders` (`Type = Purchase`) for vendor pricing, `IDocumentNumberGeneratorService` for PR/PO numbering, dynamic permission policies (`Procurement_View/Create/Edit/...`).

## UI convention (decided): follow Inventory module exactly
Procurement's UI (Index/Create/Edit/Detail across PR and PO) must copy Inventory's established layout/JS pattern verbatim, not invent new styling:
- Index: single-row toolbar (`_ShowEntries` left, keyword input + Filter dropdown + Add New right), `_DeleteConfirmModal` partial, badge `bg-label-*` status convention, `_DetailNavHeader` partial on Detail with adjacent-id Prev/Next (`IPurchaseRequisitionAdjacentQuery`/PO equivalent, same shape as `InventoryAdjacentQuery`).
- Create/Edit forms: every dropdown (Vendor, Warehouse, Item, Batch, Currency, etc.) uses `<select class="selectpicker form-control form-control-sm gr-select" data-live-search="true" data-style="btn-default" data-width="100%">` (bootstrap-select) — **never** plain `form-select`/`form-select-sm`. Reference implementation: [GoodsReceipts/Create.cshtml](../../src/Sugentra.ERP.UI/Views/GoodsReceipts/Create.cshtml).
- Dynamic line rows (PR lines, PO lines) copy the exact clone/init/destroy JS pattern from GoodsReceipts' `addLine` handler: strip `.bootstrap-select` wrapper divs and `[id]` attributes from the cloned row before re-initializing `selectpicker`, via a shared `initSelectPickers($scope)` helper (destroy-then-init, since `.selectpicker` class is stripped after first init so re-scoping must key off `.gr-select` instead).
- Input-group height-matching (a text/number input joined with a remove-line button) uses the `.unit-cost-group`-style pattern in `site.css` (radius removed on adjacent sides, button `padding-block: 0.4415rem !important` to match input height) — add a new named class per new joined-field type (e.g. `.po-line-group`) rather than reusing `.unit-cost-group` directly across unrelated fields.
- Any new CSS goes in `site.css` only — never inline `<style>` blocks, and never a bespoke `height: 34px !important` override when the actual fix is converting a field to `.gr-select` (a bespoke override was tried and reverted for Landed Cost Document's Create screen this way — see [inventory.md](inventory.md) session log).

## UI reference details (verified against GoodsReceipts Index/Create — apply verbatim, do not re-derive)

**Index filter (Goods Receipts Index.cshtml pattern):**
- Keyword input: `<input type="text" name="keyword" class="form-control form-control-sm" style="width: 250px;" placeholder="Search.." />` — placeholder text is exactly `Search..` (two dots), sits left of the Filter dropdown, inside the same `<form method="get" id="{page}FilterForm">` as the rest of the filter fields (not the `_ShowEntries` form).
- Filter is a dropdown button, not an inline row: `<button class="btn btn-sm btn-outline-primary position-relative" data-bs-toggle="dropdown" data-bs-auto-close="outside"><i class="ri ri-filter-3-line me-1"></i>Filter</button>`, with a red dot badge (`position-absolute top-0 start-100 translate-middle p-1 bg-danger border border-light rounded-circle`) shown only when any advanced filter field is non-empty (`hasAdvancedFilter` bool computed in the view from `ViewBag` fields).
- Dropdown body: `dropdown-menu dropdown-menu-end w-px-500 p-0` wrapping a borderless `card` (`shadow-none`) with `card-header`/`card-body` (2-col `row g-2` of labeled fields) /`card-footer d-flex justify-content-end gap-2` holding `Clear` (`btn btn-sm btn-label-dark waves-effect btn-filter-action`, plain link back to Index with no query params) and `Apply` (`btn btn-sm btn-primary btn-filter-action`, `type="submit"`).
- Advanced filter fields for PR/PO: Number, Vendor (dropdown for PO) / Requester (PR), Date From/Date To (Flatpickr date-only, `input-group input-group-sm {page}-date-group` + calendar icon toggle span, same as `received-date-group`), Status (`form-select form-select-sm`, plain options list, not bootstrap-select — status dropdown in a filter panel stays a native select).
- "Add New"/"Add Data" button sits at the end of the same filter `<form>`, standard `btn btn-sm btn-primary` with `ri ri-add-line`.
- On submit, JS disables any empty field before the GET request so blank params aren't sent (see `goodsReceiptsFilterForm` submit handler) — replicate this exact `disabled = true` loop per filter form.
- All these buttons/inputs are `-sm` variants so they resolve to the standard 34px control height (plan.md point 12) — never a bespoke height override.

**Header + Line validation (GoodsReceipts Create.cshtml pattern) — required for both PR and PO forms:**
- Every required header field gets `required data-required-message="..."` plus a `required-label` class on its `<label>` (adds the red asterisk styling already defined in site.css).
- Every field (header or line) has a sibling `<div class="text-danger small line-error"></div>` immediately after it to hold the inline message — line-level errors also get a dedicated class when the error is about a derived/hidden value (e.g. `.item-error` for the item-from-batch case; PR/PO would use something like `.vendor-error`/`.warehouse-error` if a similar hidden-field-derived-from-select pattern applies).
- `setFieldError(field, message)` helper writes into the closest `.line-error` sibling — reuse this helper verbatim (only the `closest()` selector list needs updating to match the new grid column classes used).
- On form submit: loop every `[required]` field, run native `checkValidity()`, call `setFieldError` per field, accumulate `valid`; `event.preventDefault()` if any invalid. Any hidden-value-derived field (like item-from-batch) gets its own explicit check block beyond native `checkValidity()`.
- Field-level errors clear live via a delegated `change input` handler scoped to the form (`$('#xForm').on('change input', 'select.x, input[required]', ...)`) — NOT bound per-element, since line rows are cloned dynamically and must inherit the same delegated behavior automatically.
- Line add/remove: clone the first `.line-row`, strip `.bootstrap-select` wrapper divs (`wrapper.replaceWith(wrapper.querySelector('select'))`) and all `[id]` attributes before appending, then call `initSelectPickers($(clone))`. Remove is delegated on the lines container (`e.target.closest('.remove-line')`), and must refuse to remove the last remaining row (`if (lines.children.length > 1)`).
- `initSelectPickers`/`rerenderSelectPicker` helpers (destroy-then-init) are copied as-is — do not reinitialize plain `.selectpicker()` without the destroy check, or bfcache/back-navigation will double-wrap selects.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- Procurement owns Purchase Requisition/PO creation and its approval gate — entirely internal to Procurement, no Inventory involvement.
- When a Goods Receipt is posted (supplier delivery scanned in), Procurement calls **Inventory's** `IGoodsReceiptStockService.ReceiveStockAsync(itemId, batchInfo, qty, warehouseId)` (declared in `Shared/Contracts/`, implemented by Inventory). This is the only call Procurement makes into Inventory.
- Procurement must pass through whatever batch/legality metadata (batch code, SVLK/FSC document refs) it collected at receiving, since Inventory's `Inventory_Batches` table needs it and Procurement should not need to read/write Inventory's tables directly (module isolation).
- Landed cost inputs (freight/insurance/handling/duty) captured during Procurement's receiving step should also be passed through the same contract call (or a dedicated request DTO) so Inventory can populate `Inventory_LandedCostAllocations` — exact DTO shape to be finalized when Procurement is built.

## Deviations from the original plan / implemented but undocumented above

- **Goods Receipt ↔ PO link: NOT implemented.** `Inventory_GoodsReceipts`/`CreateGoodsReceiptRequest` has no `PurchaseOrderId` at all, and `IPurchaseOrderReceiptService.ApplyReceiptAsync` (implemented by `PurchaseOrderUseCase`, updates `ReceivedQuantity`/rolls PO status to `PartiallyReceived`/`FullyReceived`) is never called from Inventory's `GoodsReceiptUseCase`. So PO `ReceivedQuantity` currently never gets filled by any real flow — it only changes if/when this wiring is added later. Don't tell users it "gets filled by posting a Goods Receipt" without also flagging this gap.
- **PO line ↔ PR line sourcing (`Procurement_PurchaseOrderLineSources`)** — not in the original plan at all. A PO can be created from multiple Approved PRs at once; same item+warehouse across PRs is merged into one PO line, and this table records the per-PR-line quantity breakdown for that merged line (many-to-many, since one PO line can pull from several PR lines and one PR line can be split across several POs).
- **Partial ordering / remaining-quantity enforcement** — a PR line can be spread across multiple POs over time. `PurchaseOrderLineSourceRepository.GetOrderedQuantityByRequisitionLineIdsAsync` sums already-ordered qty per PR line (excluding the current order's own rows on Update); `PurchaseOrderUseCase.ValidateSourceQuantitiesAsync` rejects a Create/Update if requested qty for any PR line exceeds `PR line qty − already ordered elsewhere`. Manual (non-PR) PO lines are unrestricted.
- **`PurchaseOrderUseCase.InferMissingSourcesAsync`** — best-effort fallback that reconstructs a PO line's `Sources` by matching ItemId against the selected PRs' lines when the client submits a line with no `Sources` breakdown. Exists mainly so old/manual submissions still get *some* source tracking, but it allocates against each PR line's full original quantity (not the remaining unordered amount) — known to overallocate if not all PR lines are already-partially-consumed. Prefer always sending an explicit `Sources` breakdown from the client instead of relying on this.
- **PR `OrderStatus` (`NotOrdered`/`PartiallyOrdered`/`FullyOrdered`)** — computed dynamically in `PurchaseRequisitionUseCase.ToResponseAsync` from `GetOrderedQuantityByRequisitionIdsAsync` vs. the PR's total requested qty; not persisted. Displayed via `StatusBadgeHelper.GetOrderStatus` in the UI (PR Index/Detail), and used to hide fully-ordered PRs from the PO Create page's Source Requisition multi-select.
- **PO Create "Max Qty" column** — client-side (`Create.cshtml`) shows the remaining orderable qty per merged line and blocks Save if the typed quantity exceeds it (mirrors the server-side `ValidateSourceQuantitiesAsync` check so the user gets immediate feedback instead of a round-trip error).
- **Known gap: PO Edit page has none of the above.** `Edit.cshtml`/`PurchaseOrdersController.Edit` never send a `sourcesJson`/Max-Qty breakdown, and `PurchaseOrderUseCase.UpdateAsync` doesn't call `InferMissingSourcesAsync` as a fallback either — editing and re-saving a Draft PO that came from a PR silently wipes its `PurchaseOrderLineSources` rows. Not yet fixed; needs the same sources/Max-Qty support added to Edit before this is safe to use on PR-sourced POs.
- **Detail view**: per-line PR source badges only render when a PO has more than one source PR (`SourceRequisitions.Count > 1`) — a single-PR PO doesn't repeat the (redundant) PR label on every line.
- **Client bug fixed**: `Create.cshtml` was posting `purchaseRequisitionId` as a JSON string (from a `<select>` value) inside `sourcesJson`; `System.Text.Json`'s default strict mode silently failed to bind it to `long?` and fell through to `InferMissingSourcesAsync`'s naive full-quantity allocation. Fixed by casting to `Number(...)` client-side and adding `JsonNumberHandling.AllowReadingFromString` server-side as a second line of defense.

## Planned (not implemented yet): Post-Approval Correction Workflow for PR/PO

> Design-only, captured ahead of implementation so the intent survives across sessions. Nothing below exists in code yet — do not assume any of these statuses/endpoints/tables are queryable until a session log entry says otherwise.

**Problem**: once a PR/PO is fully Approved (and a PO especially once received against), it must stay locked for audit-trail/integrity reasons, but real-world mistakes (wrong qty, wrong spec, wrong price, vendor short-ships) still need a sanctioned way to correct — without ever mutating an approved document's own rows.

### 0. Status model: two dimensions, not one

Both PR and PO get a second, independent status axis instead of overloading the existing single `Status` column:

- **Approval Status** (unchanged, existing): `Draft` → `PendingApproval` → `Approved` / `Rejected`.
- **Lifecycle/Fulfillment Status** (new axis, only meaningful once `Approved`): `Open` (= today's `NotOrdered`/nothing-received-yet baseline) → `PartiallyOrdered`/`PartiallyReceived` → `FullyOrdered`/`FullyReceived`, plus the new terminal states `Cancelled`, `Closed`, `Superseded`. Needs its own column (e.g. `LifecycleStatus`) rather than replacing the existing `Status` column used for approval, so a PO can be simultaneously "Approved" (approval axis) and "Closed" (lifecycle axis) without ambiguity.

Definitions:
- **Cancelled**: killed before any real progress (no GR, no invoice) — document dead entirely.
- **Superseded**: auto-set on the OLD PO the moment a Revision is created from it — read-only audit reference, never re-activated.
- **Closed**: force-stopped mid-flight (vendor can't deliver the rest, contract cut short) — whatever was already received/ordered stays valid, only the undelivered/unordered remainder is released.

#### PO lifecycle transition table

The allowed-transition graph is now a real table — [0011_Procurement_StatusTransitions.sql](../../src/Sugentra.ERP.Migrator/Scripts/Procurement/0011_Procurement_StatusTransitions.sql) — `Procurement_StatusTransitions` (`CategoryGroup` = `PurchaseOrder`/`PurchaseRequisition`, `FromStatus`, `ToStatus`, `ConditionDescription`, `IsSystemTriggered`), instead of only living in this markdown doc. This keeps the rule set data-driven (queryable/editable without a code change) rather than hardcoded as a big if/switch inside `PurchaseOrderUseCase`/`PurchaseRequisitionUseCase`. **Note: this table is seeded ahead of the feature itself — nothing reads/writes it yet, and the `LifecycleStatus` column it validates against doesn't exist on the entities yet either.** Seeded rows:

| CategoryGroup | From | To | Condition | System-triggered |
|---|---|---|---|---|
| PurchaseOrder | Draft/PendingApproval | Approved | Full approval hierarchy passed | no |
| PurchaseOrder | Draft/PendingApproval | Cancelled | Cancelled by creator/Procurement Manager before approval | no |
| PurchaseOrder | Approved | PartiallyReceived | Warehouse posts a GR for part of the qty | yes |
| PurchaseOrder | Approved | FullyReceived | Warehouse posts a GR for 100% of the qty | yes |
| PurchaseOrder | Approved | Cancelled | No GR and no Invoice exist yet | no |
| PurchaseOrder | Approved / PartiallyReceived | Superseded | System-set automatically when "Revise PO" is confirmed and the new PO is created | yes |
| PurchaseOrder | PartiallyReceived | FullyReceived | Warehouse receives the remaining qty | yes |
| PurchaseOrder | PartiallyReceived | Closed | Remaining qty force-closed by Admin/Manager (vendor won't ship the rest) | no |
| PurchaseRequisition | Approved | Closed | Remaining un-ordered line quantity force-closed by Admin/Manager | no |

Closing a `PartiallyReceived` PO must release its undelivered remainder's `PurchaseOrderLineSources` consumption back to the source PR line(s), so the PR line reopens (`FullyOrdered` → `PartiallyOrdered`/`Open`) and can be re-PO'd elsewhere.

#### PR lifecycle states
1. **NotOrdered** — Approved, nothing pulled into a PO yet (existing).
2. **PartiallyOrdered** — some qty converted to PO(s), remainder still open (existing).
3. **FullyOrdered** — all qty consumed by PO(s) (existing).
4. **Closed** — remaining un-ordered qty force-closed manually (new). Example: PR asked for 10, 6 already went to a PO, project cancelled → remaining 4 gets `Closed`, line ends up reported as a `PartiallyOrdered & Closed` combination (still shows the 6 that were ordered, but the 4 no longer counts as open/orderable). This can be a per-line flag/quantity (`ClosedQuantity`) rather than a single PR-level enum, since a PR has multiple lines that can each be closed independently — no new revision/versioning mechanic needed here, just a flag + history entry via the audit log.

### 1. PR-level correction
- No direct edit of an Approved PR, ever (already true today — PR has no Edit action once Approved).
- **PR Cancellation / Close**: new action (`PurchaseRequisition_Cancel` — permission TBD) that sets `ClosedQuantity` on the *unordered remainder* of an Approved PR's lines (i.e. `Quantity − OrderedQuantity` per line) without touching the part already converted to PO(s). `OrderStatus` calculation must subtract `ClosedQuantity` too, so it stops counting the closed remainder as orderable. Reason text required — logged via existing `IAuditLogService`, not a new table (per your instruction: treat this as a flag + history entry, no separate versioning mechanic).
- **Adjustment PR**: no new mechanic needed — this is just "create a normal new PR" for the shortfall/correction; already fully supported. Document this as the sanctioned pattern in user-facing help text rather than building a special "linked adjustment PR" concept, to avoid a parallel PR type.

### 2. PO-level correction

**A. Not yet sent to vendor / just approved, no GR yet**
- **PO Revision/Amendment**: a "Revise" action on an Approved, not-yet-fully-received PO. Creates a new PO row referencing the original (`Procurement_PurchaseOrders.RevisesPurchaseOrderId` nullable FK), copies lines/sources, sets old PO `LifecycleStatus = "Superseded"`. Numbering scheme for the new PO is out of scope for now (plain next-sequence number is fine) — just make sure the old→new link is queryable so Detail can show "Revised by PO/2026/00xx".
  - Revision goes through the same approval flow (`IApprovalService`/`IApprovalDocumentHandler`) as a normal PO, since it can change vendor-facing terms (qty/price).
  - Because sources are consumed via `Procurement_PurchaseOrderLineSources`, revising must carefully carry over (not double-count) each line's PR-line allocation — the old PO's sources should be excluded from `GetOrderedQuantityByRequisitionLineIdsAsync` once `LifecycleStatus = "Superseded"`, same way a soft-deleted PO already would be, so re-validating the new PO's quantities against PR remaining doesn't double-subtract.

**B. Already sent to vendor / goods already received**
- **Cancel PO**: only allowed if no Goods Receipt exists against it yet (once GR↔PO linking exists — see the "NOT implemented" gap above, this feature depends on that landing first). Cancelling must release the PO's `PurchaseOrderLineSources` consumption so the source PR line(s) become orderable again (mirrors what a soft-delete already does for Draft POs; Approved/sent POs currently can't be deleted at all).
- **Close PO**: for a PO that will never be fully delivered (vendor short-ships permanently) — closes the undelivered remainder (`Quantity − ReceivedQuantity` per line) without cancelling what was already received. The closed remainder's PR-line consumption should be released back to the source PR the same way, so the shortfall can be re-PO'd via a new/adjustment PR if still needed. See transition table above (`PartiallyReceived → Closed`).
- **Return to Vendor (Retur)**: new document type, modeled after `Inventory_GoodsReceipts` but reversed (references a posted GR + PO, negative stock movement via `IGoodsReceiptStockService` or a new contract method). Out of scope until GR↔PO linking exists.
- **Debit Note / Credit Note**: financial correction, belongs to the future Finance/AP module (Vendor Bill/Invoice Matching), same "out of scope for v1" reasoning as Vendor Bill above — revisit once Finance module scope is defined.

### 3. Cross-cutting features to add alongside the above
- **Audit Trail**: already have `IAuditLogService.LogAsync` — every new action (Cancel/Close/Revise) must call it with an explicit reason string (add a `string? Reason` param to whatever request DTOs these actions use, since today's audit log entries don't force a reason).
- **Cancel & Close mechanism**: as described above, gated on GR existence once that link is built.
- **Adjustment/Variation Order**: represented as a PO Revision (2A above) rather than a separate VO document type, to avoid a fourth procurement document type — revisit if a lighter-weight "just bump qty/price without a full re-approval" flow turns out to be needed.
- **Permissions**: new policy codes needed — `PurchaseOrder_Revise`, `PurchaseOrder_Cancel`, `PurchaseOrder_Close`, `PurchaseRequisition_Cancel` — assignable to roles same as any other permission (dynamic policy provider already supports arbitrary new codes with no extra registration).
- **Reporting impact**: anywhere PO/PR counts are aggregated (dashboards, if any) must exclude `Cancelled`/`Superseded`/`Closed` from "pending/active" buckets once this ships.

### Open questions to decide before building (not yet answered)
- How many times can a PO be revised — single-level (`RevisesPurchaseOrderId` pointing at the immediate predecessor only) or a full chain? Leaning single-level unless a real need for multi-hop history shows up.
- Which fields are allowed to change on a Revision (qty/price/date only, or can Vendor change too)? If Vendor can change, it starts looking like "cancel + new PO" instead of a revision — needs a decision.
- Does Revise/Cancel/Close require full re-approval, or a lighter-weight approval (and if lighter, on what threshold)?

### Suggested build order (when this is picked up)
1. Add `LifecycleStatus` column + transition table enforcement for PO and PR (this is the foundation everything else sits on).
2. Goods Receipt ↔ PO link (prerequisite for B-scenarios above, and already flagged as an existing gap).
3. PR Cancel/Close (remainder-only, per-line `ClosedQuantity`), since it's isolated and needed regardless of PO revision work.
4. PO Revision/Amendment (A-scenario) — needs the PR-line-release logic from step 3 as a building block.
5. PO Cancel/Close (B-scenario) — needs step 2.
6. Return to Vendor + Debit/Credit Note — defer to whenever Finance module work starts.
