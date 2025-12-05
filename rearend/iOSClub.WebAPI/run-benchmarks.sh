#!/bin/bash

# 添加.NET工具到PATH
export PATH="$PATH:/Users/luckyfish/.dotnet/tools"

# 运行基准测试
dotnet-benchmark "iOSClub.Tests/bin/Release/net10.0/iOSClub.Tests.dll" --filter "iOSClub.Tests.PerformanceTests.SecurityPerformanceTests"
