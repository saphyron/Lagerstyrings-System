# Test Report

- **Timestamp:** 2025-10-29_12-49-19
- **Project:** C:\Users\JohnGrandtMarkvardHø\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\Test-First.csproj
- **Results directory:** C:\Users\JohnGrandtMarkvardHø\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\Reports\2025-10-29_12-49-19
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
| 27 | 26 | 1 | 0 |

## Failed tests

### Tests.Spectests.OrdersContractTests::PUT /orders/{id} → 204

**Message:**

```
Expected r.StatusCode to be HttpStatusCode.NoContent {value: 204}, but found HttpStatusCode.InternalServerError {value: 500}.
```

**Expected vs Actual (parsed):**

| Expected | Actual |
|----------|--------|
| HttpStatusCode.NoContent {value: 204}, | HttpStatusCode |

**StackTrace:**

```
   at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.EnumAssertions`2.Be(TEnum expected, String because, Object[] becauseArgs)
   at Tests.Spectests.OrdersContractTests.Update() in C:\Users\JohnGrandtMarkvardHø\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\src\Spectests\OrdersContractTests.cs:line 80
   at Tests.Spectests.OrdersContractTests.Update() in C:\Users\JohnGrandtMarkvardHø\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\src\Spectests\OrdersContractTests.cs:line 80
--- End of stack trace from previous location ---
```
**Duration:** 00:00:00.0742965


## Passed tests

| Test | Duration | Notes |
|------|----------|-------|
| Tests.Spectests.WarehousesContractTests::PUT /warehouses/{id} → 204 | 00:00:00.0216903 | Matched expected behaviour. |
| Tests.Spectests.ProductsContractTests::DELETE /products/{id} → 204 | 00:00:00.2540023 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::POST /auth/users → 201 Created | 00:00:00.0278695 | Matched expected behaviour. |
| Tests.Spectests.OrdersContractTests::DELETE /orders/{id} → 204 | 00:00:00.0599899 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::GET /auth/users → 200 (with JWT) | 00:00:00.2792065 | Matched expected behaviour. |
| Tests.Spectests.AuthRolesContractTests::GET /auth/roles → 200 (JWT) | 00:00:00.0122139 | Matched expected behaviour. |
| Tests.Spectests.ProductsContractTests::POST /products → 201 | 00:00:00.0278809 | Matched expected behaviour. |
| Tests.Spectests.WarehousesContractTests::POST /warehouses → 201 | 00:00:00.0185523 | Matched expected behaviour. |
| Tests.Spectests.AuthRolesContractTests::POST /auth/roles → 201 or 409 (JWT) | 00:00:00.0162237 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::PUT /auth/users/{id} → 204 NoContent | 00:00:00.0324300 | Matched expected behaviour. |
| Tests.Spectests.ProductsContractTests::PUT /products/{id} → 204 | 00:00:00.0146369 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::POST /auth/users/login → 200 + token | 00:00:00.0223816 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::GET /auth/users/{id} → 200/404 (with JWT) | 00:00:00.0120683 | Matched expected behaviour. |
| Tests.Spectests.WarehousesContractTests::GET /warehouses → 200 list | 00:00:00.0050934 | Matched expected behaviour. |
| Tests.Spectests.WarehousesContractTests::GET /warehouses/{id} → 200/404 | 00:00:00.2630826 | Matched expected behaviour. |
| Tests.Spectests.OrdersContractTests::GET /orders/by-user/{userId} → 200 | 00:00:00.2540189 | Matched expected behaviour. |
| Tests.Spectests.AuthRolesContractTests::DELETE /auth/roles/{authEnum} → 204/409 (JWT) | 00:00:00.0233678 | Matched expected behaviour. |
| Tests.Spectests.ProductsContractTests::GET /products → 200 list | 00:00:00.0081366 | Matched expected behaviour. |
| Tests.Spectests.WarehousesContractTests::DELETE /warehouses/{id} → 204 | 00:00:00.0176557 | Matched expected behaviour. |
| Tests.Spectests.AuthRolesContractTests::GET /auth/roles/{authEnum} → 200/404 (JWT) | 00:00:00.2635723 | Matched expected behaviour. |
| Tests.Spectests.OrdersContractTests::PATCH /orders/{id}/status → 204 | 00:00:00.0946520 | Matched expected behaviour. |
| Tests.Spectests.AuthRolesContractTests::PUT /auth/roles/{authEnum} → 204 (JWT) | 00:00:00.0388896 | Matched expected behaviour. |
| Tests.Spectests.ProductsContractTests::GET /products/{id} → 200/404 | 00:00:00.0126788 | Matched expected behaviour. |
| Tests.Spectests.OrdersContractTests::POST /orders → 201 | 00:00:00.0208723 | Matched expected behaviour. |
| Tests.Spectests.OrdersContractTests::GET /orders/{id} → 200/404 | 00:00:00.0071071 | Matched expected behaviour. |
| Tests.Spectests.UsersContractTests::DELETE /auth/users/{id} → 204/404 | 00:00:00.0180741 | Matched expected behaviour. |

## Console output (tail)

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  Determining projects to restore...
  All projects are up-to-date for restore.
C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\src\Helpers\DbUtility.cs(52,12): warning CS8603: Possible null reference return. [C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\Test-First.csproj]
  Test-First -> C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll
Test run for C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.14.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
dotnet.exe : [xUnit.net 00:00:01.24]     PUT /orders/{id} ÔåÆ 204 [FAIL]
At C:\Users\JohnGrandtMarkvardHø\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\Scripts\Tests.ps1:76 char:15
+ $testOutput = & dotnet @argList 2>&1
+               ~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: ([xUnit.net 00:0... ÔåÆ 204 [FAIL]:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
  Failed PUT /orders/{id} ÔåÆ 204 [74 ms]
  Error Message:
   Expected r.StatusCode to be HttpStatusCode.NoContent {value: 204}, but found HttpStatusCode.InternalServerError {value: 500}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.EnumAssertions`2.Be(TEnum expected, String because, Object[] becauseArgs)
   at Tests.Spectests.OrdersContractTests.Update() in C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\src\Spectests\OrdersContractTests.cs:line 80
   at Tests.Spectests.OrdersContractTests.Update() in C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\tests\src\Spectests\OrdersContractTests.cs:line 80
--- End of stack trace from previous location ---
Results File: C:\Users\JohnGrandtMarkvardH├©\Desktop\Opgaver\Uge 6 og 7\Lagerstyrings-System\Reports\2025-10-29_12-49-19\TestResults.trx

Failed!  - Failed:     1, Passed:    26, Skipped:     0, Total:    27, Duration: 537 ms - Test-First.dll (net8.0)
```
