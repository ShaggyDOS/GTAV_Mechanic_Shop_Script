# Glass Heroes Business Manager v0.3.4 RC1

EXPERIMENTAL — GTA TESTING NOT YET COMPLETED.

Reviewed source commit: e312d8c9500f4a990eb993b82e8e76b55711d3de
Source SHA256: 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120
Branch: feature/shvdn3-340-inventory-procurement.

Includes all v0.3.4 inventory/procurement work plus the four owner-accepted Issue36 maintenance fixes. Owner YES accepts the combined integration; subsequent “Preapproved” authorizes preparation of this testing package only. No tag, public release, merge or installation.

Implemented: on-hand/reserved/free/incoming inventory and dual unit/parcel capacity; reorder advice; explicit mixed-order review/confirmation; order identity and durable last receipt/cancellation; bounded Mule recovery; save-or-rollback cargo/pallet/receipt changes; existing nearby/offsite manager LOCAL/EXPRESS integration; measured/cached footer layout; clearer shared mechanic routes; owned READY wholesale map locator and explicit GPS; canonical receptionist/manager route editors.

Actual final verification: 954/954 behavior checks, including 212 navigation; 21/21 static, 13/13 inventory structural; genuine net48/C#7.3 SHVDN3 3.6.0 Release zero errors/two inherited warnings CS0169 and CS0649. Clean-extraction build is recorded separately in PACKAGE_VERIFICATION.json accompanying the ZIP.

Limits: GTA/Windows gameplay/Enhanced runtime testing NOT RUN; one pending order and last outcome; historical parcel slots remain earned/consumed after stock use; nearby manager needs configured physical route; wholesale carry reload retains warehouse recovery; no new prices/items, physical damaged-car repairs or crash fix.
