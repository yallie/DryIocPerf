# DryIoc v4 vs DryIoc v2

### v2.12.6.0

```log
D:\Externals\DryIocPerf\Bin\v2\net46>DryIoc2Perf.exe
[02:53:07 INF] Starting up...
[02:53:07 INF] Started.
[02:53:07 INF] Hello there. Starting the benchmark.
[02:53:07 INF] Imported web services: 104
[02:53:07 INF] GetNow: 2021-07-13T02:53:07.0000000+03:00
[02:53:07 INF] Warming up...
[02:53:08 INF] Running the benchmark...
[02:53:08 INF] Time elapsed: 00:00:00.0756021
```

### v3.0.2.0

```log
D:\Externals\DryIocPerf\Bin\v3\net46>DryIoc3Perf.exe
[02:53:13 INF] Starting up...
[02:53:14 INF] Started.
[02:53:14 INF] Hello there. Starting the benchmark.
[02:53:14 INF] Imported web services: 104
[02:53:14 INF] GetNow: 2021-07-13T02:53:14.0000000+03:00
[02:53:14 INF] Warming up...
[02:53:14 INF] Running the benchmark...
[02:54:20 INF] Time elapsed: 00:01:05.5922111
```

### v4.8.1.0

```log
D:\Externals\DryIocPerf\Bin\v4\net46>DryIoc4Perf.exe
[03:04:06 INF] Starting up...
[03:04:06 INF] Started.
[03:04:06 INF] Hello there. Starting the benchmark.
[03:04:06 INF] Imported web services: 104
[03:04:06 INF] GetNow: 2021-07-13T03:04:06.0000000+03:00
[03:04:06 INF] Warming up...
[03:04:06 INF] Running the benchmark...
[03:04:12 INF] Time elapsed: 00:00:05.4144036
```

* v4.8.1 performs ~72 times slower than v2.12.6
* v3.0.2 performs ~876 times slower than v2.12.6
