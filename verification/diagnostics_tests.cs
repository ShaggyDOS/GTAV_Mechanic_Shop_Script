// Embedded by run_diagnostics_tests.py alongside the production logger.
internal static class DiagnosticsTests
{
    private static int _passed;
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("FAIL: " + description);
    }
    private static void Test(string name, Action body)
    {
        body();
        _passed++;
        Console.WriteLine("PASS " + name);
    }
    private static DateTime Start => new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc);
    private static void Main()
    {
        Test("30 minutes of stable idle writes exactly one state checkpoint", () => {
            var now = Start;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, lines.Add);
            for (int i = 0; i <= 1800; i++) {
                now = Start.AddSeconds(i);
                log.RecordTransition(true, false, true, false, 0, 0, 0);
            }
            Check(lines.Count == 1, "stable state must not generate a heartbeat");
            Check(lines[0].Contains("shift=True remote=False staffInRange=True"), "readable state");
        });
        Test("rapid state changes coalesce and retain latest observed state", () => {
            var now = Start;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, lines.Add);
            log.RecordTransition(false, false, false, false, 0, 0, 0);
            for (int i = 1; i < 100; i++) {
                now = Start.AddMilliseconds(i * 99);
                log.RecordTransition(true, true, true, true, 6, 3, 2);
            }
            Check(lines.Count == 1, "rate limit rapid transitions");
            now = Start.AddSeconds(10);
            log.RecordTransition(true, true, true, true, 6, 3, 2);
            Check(lines.Count == 2 && lines[1].Contains("supplierStage=6 customers=3 remoteJobs=2"), "latest state recorded");
        });
        Test("all observed state dimensions can independently trigger a checkpoint", () => {
            var now = Start;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, lines.Add);
            log.RecordTransition(false, false, false, false, 0, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, false, false, false, 0, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, false, false, 0, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, true, false, 0, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, true, true, 0, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, true, true, 1, 0, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, true, true, 1, 1, 0);
            now = now.AddSeconds(10); log.RecordTransition(true, true, true, true, 1, 1, 1);
            Check(lines.Count == 8, "no state dimension lost");
        });
        Test("10,000 repeated or alternating errors produce one write per 10 seconds", () => {
            var now = Start;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, lines.Add);
            var error = new InvalidOperationException("injected failure");
            for (int i = 0; i < 10000; i++) log.Write(i % 2 == 0 ? "staff" : "supplier", error);
            Check(lines.Count == 1, "bound alternating errors, not just identical messages");
            now = now.AddSeconds(10);
            Check(log.Write("Tick / staff visuals", error), "next window admits error");
            Check(lines.Count == 2 && lines[1].Contains("suppressed errors=9999"), "summarize suppressed errors");
            now = now.AddSeconds(10);
            log.Write("next", error);
            Check(!lines[2].Contains("suppressed errors="), "suppression count resets");
        });
        Test("startup and teardown checkpoints remain available during error throttle", () => {
            var now = Start;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, lines.Add);
            log.Write("error", new Exception("test"));
            log.Write("BOOT ready", null);
            log.Write("LIFECYCLE abort cleanup completed", null);
            Check(lines.Count == 3, "ordinary checkpoints unaffected by exception throttle");
        });
        Test("disk failures back off for 30 seconds and unchanged state retries", () => {
            var now = Start;
            int attempts = 0;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => now, line => {
                attempts++;
                if (attempts == 1) throw new System.IO.IOException("disk unavailable");
                lines.Add(line);
            });
            for (int i = 0; i < 30; i++) {
                now = Start.AddSeconds(i);
                log.RecordTransition(true, false, true, false, 0, 0, 0);
                log.Write("checkpoint", null);
            }
            Check(attempts == 1, "no repeated I/O while unavailable");
            now = Start.AddSeconds(30);
            log.RecordTransition(true, false, true, false, 0, 0, 0);
            Check(attempts == 2 && lines.Count == 1, "retry same state after recovery");
        });
        Test("reentrant writer cannot recurse through logger", () => {
            var now = Start;
            int attempts = 0;
            DiagnosticLog log = null;
            log = new DiagnosticLog(() => now, line => {
                attempts++;
                Check(!log.Write("recursive callback", null), "reject nested logging");
            });
            Check(log.Write("outer", null), "outer write succeeds");
            Check(attempts == 1, "exactly one writer invocation");
        });
        Test("clock failures cannot escape and writer guard recovers", () => {
            bool fail = true;
            var lines = new List<string>();
            var log = new DiagnosticLog(() => fail ? throw new Exception("clock failure") : Start, lines.Add);
            Check(!log.Write("clock unavailable", null), "clock failure contained");
            fail = false;
            Check(log.Write("recovered", null) && lines.Count == 1, "guard reset in finally");
        });
        Test("UTC timestamp and tick context survive exception formatting", () => {
            var lines = new List<string>();
            var log = new DiagnosticLog(() => Start, lines.Add);
            log.Write("Tick / warehouse visuals", new Exception("injected native wrapper error"));
            Check(lines[0].StartsWith("2026-10-22T00:00:00Z | Tick / warehouse visuals | "), "UTC and tick context");
            Check(lines[0].Contains("injected native wrapper error"), "exception details retained");
        });
        Console.WriteLine($"{_passed}/{_passed} diagnostic behavior tests passed; GTA gameplay NOT RUN.");
    }
}
