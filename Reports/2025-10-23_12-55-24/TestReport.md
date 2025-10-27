# Test Report

- **Timestamp:** 2025-10-23_12-55-24
- **Project:** C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\Test-First.csproj
- **Results directory:** C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Reports\2025-10-23_12-55-24
- **ORDERS_URL:** http://localhost:5107
- **ORDERS_CLI:** 
- **CATALOG_URL:** http://localhost:5107
- **INVENTORY_URL:** http://localhost:5107
- **PRODUCTS_URL:** http://localhost:5107
- **AUTH_URL:** http://localhost:5107
- **ConnectionStrings__Default:** `Server=localhost;Database=LSSDb;User Id=LSSUser;Password=LSS-P@ssw0rd;Encrypt=True;TrustServerCertificate=True;`

## Summary

| Total | Passed | Failed | Skipped |
|------:|------:|------:|--------:|
| 8 | 5 | 3 | 0 |

## Failed tests

### Tests.Spectests.CatalogContractTests::Create warehouse via service → row exists in DB

**Message:**

```
Expected res.IsSuccessStatusCode to be true, but found False.
```

**Expected vs Actual (parsed):**

| Expected | Actual |
|----------|--------|
| true, | False |

**StackTrace:**

```
   at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
   at Tests.Spectests.CatalogContractTests.CreateWarehouse() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\CatelogContractTests.cs:line 39
--- End of stack trace from previous location ---
```
**Duration:** 00:00:00.0532781


### Tests.Spectests.CatalogContractTests::Create product via service → row exists in DB

**Message:**

```
Expected res.IsSuccessStatusCode to be true, but found False.
```

**Expected vs Actual (parsed):**

| Expected | Actual |
|----------|--------|
| true, | False |

**StackTrace:**

```
   at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
   at Tests.Spectests.CatalogContractTests.CreateProduct() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\CatelogContractTests.cs:line 25
--- End of stack trace from previous location ---
```
**Duration:** 00:00:00.0032837


### Tests.Spectests.OrdersInventoryContractTests::Sales: stock decreases per item and one log row per item

**Message:**

```
System.InvalidOperationException : HTTP 404:
```
**StackTrace:**

```
   at Tests.Helpers.OrdersHttpInvoker.CreateOrderAsync(String jsonPayload) in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\OrdersHttpInvoker.cs:line 25
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 59
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 86
--- End of stack trace from previous location ---
```
**Duration:** 00:00:00.1043944


## Passed tests

| Test | Duration | Notes |
|------|----------|-------|
| Tests.Spectests.AuthContractTests::Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec | 00:00:00.0946192 | Matched expected behaviour. |
| Tests.Spectests.AuthContractTests::Tests.Spectests.AuthContractTests.Protected_endpoints_require_JWT | 00:00:00.0350498 | Matched expected behaviour. |
| Tests.Spectests.OrdersInventoryContractTests::Sales with insufficient stock should return error and change nothing | 00:00:00.0105171 | Matched expected behaviour. |
| Tests.Spectests.AuthContractTests::Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized | 00:00:00.0094011 | Matched expected behaviour. |
| Tests.Spectests.AuthContractTests::Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info | 00:00:00.0031352 | Matched expected behaviour. |

## Console output (tail)

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  Determining projects to restore...
  All projects are up-to-date for restore.
  Test-First -> C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll
Test run for C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.14.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
dotnet.exe : [xUnit.net 00:00:00.13]     Create warehouse via service ΓåÆ row exists in DB [FAIL]
At C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Scripts\Tests.ps1:76 char:15
+ $testOutput = & dotnet @argList 2>&1
+               ~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: ([xUnit.net 00:0...ts in DB [FAIL]:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
[xUnit.net 00:00:00.13]     Create product via service ΓåÆ row exists in DB [FAIL]
[xUnit.net 00:00:00.18]     Sales: stock decreases per item and one log row per item [FAIL]
  Failed Create warehouse via service ΓåÆ row exists in DB [53 ms]
  Error Message:
   Expected res.IsSuccessStatusCode to be true, but found False.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
   at Tests.Spectests.CatalogContractTests.CreateWarehouse() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\CatelogContractTests.cs:line 39
--- End of stack trace from previous location ---
  Failed Create product via service ΓåÆ row exists in DB [3 ms]
  Error Message:
   Expected res.IsSuccessStatusCode to be true, but found False.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
   at Tests.Spectests.CatalogContractTests.CreateProduct() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\CatelogContractTests.cs:line 25
--- End of stack trace from previous location ---
  Failed Sales: stock decreases per item and one log row per item [104 ms]
  Error Message:
   System.InvalidOperationException : HTTP 404: 
  Stack Trace:
     at Tests.Helpers.OrdersHttpInvoker.CreateOrderAsync(String jsonPayload) in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\OrdersHttpInvoker.cs:line 25
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 59
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 86
--- End of stack trace from previous location ---
Results File: C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Reports\2025-10-23_12-55-24\TestResults.trx

Failed!  - Failed:     3, Passed:     5, Skipped:     0, Total:     8, Duration: 144 ms - Test-First.dll (net8.0)
```
