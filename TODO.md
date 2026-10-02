# TODO (owner decides when items are done)
## Foundations (first)
- [~] (implemented, awaiting Ozgur) Scan hygiene: remove sample_data/golden from real-campaign scan paths; de-duplicate live vs archive files; normalise CombatEvents date.
- [~] (implemented, awaiting Ozgur) Implement `.wic` vault write/read per SPEC §5; wire wizard + Continue.
- [~] (implemented, awaiting Ozgur) Allied-only: remove Japanese option from wizard; header shows real campaign name/side.
## Cleanup
- [~] (implemented, awaiting Ozgur) Delete War Room items Intelligence Summary, SIGINT Map, TPIE, Predictive Models + dead code.
- [ ] Decide: date-range filter modal (TemporalFilter/Calendar) now has no entry point; re-home or delete.
- [~] (implemented, awaiting Ozgur) Remove AI-endpoint settings (offline rule).
- [~] (implemented, awaiting Ozgur) Prune/decide unused pillar services: decided 2026-10-02 = stop running, keep code + tests (parked).
## Later
- [ ] Re-home deleted functions on new pages (owner to specify).
- [ ] PWS field verification against real saves.
