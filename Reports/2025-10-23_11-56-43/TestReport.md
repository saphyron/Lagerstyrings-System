# Test Report

- **Timestamp:** 2025-10-23_11-56-43
- **Project:** C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\Test-First.csproj
- **Results directory:** C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Reports\2025-10-23_11-56-43
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
| 8 | 1 | 7 | 0 |

## Failed tests

### Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info

**Message:**

```
Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
```

**StackTrace:**

```
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 76
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

```

### Sales: stock decreases per item and one log row per item

**Message:**

```
Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
```

**StackTrace:**

```
   at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, SqlCommand command, Boolean callerHasConnectionLock, Boolean asyncClose)
   at Microsoft.Data.SqlClient.TdsParser.Connect(ServerInfo serverInfo, SqlInternalConnectionTds connHandler, TimeoutTimer timeout, SqlConnectionString connectionOptions, Boolean withFailover)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.AttemptOneLogin(ServerInfo serverInfo, String newPassword, SecureString newSecurePassword, TimeoutTimer timeout, Boolean withFailover)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.LoginNoFailover(ServerInfo serverInfo, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance, SqlConnectionString connectionOptions, SqlCredential credential, TimeoutTimer timeout)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.OpenLoginEnlist(TimeoutTimer timeout, SqlConnectionString connectionOptions, SqlCredential credential, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds..ctor(DbConnectionPoolIdentity identity, SqlConnectionString connectionOptions, SqlCredential credential, Object providerInfo, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance, SqlConnectionString userConnectionOptions, SessionData reconnectSessionData, Boolean applyTransientFaultHandling, String accessToken, DbConnectionPool pool, Func`3 accessTokenCallback)
   at Microsoft.Data.SqlClient.SqlConnectionFactory.CreateConnection(DbConnectionOptions options, DbConnectionPoolKey poolKey, Object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningConnection, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.CreatePooledConnection(DbConnectionPool pool, DbConnection owningObject, DbConnectionOptions options, DbConnectionPoolKey poolKey, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionPool.CreateObject(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.UserCreateRequest(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.WaitForPendingOpen()
--- End of stack trace from previous location ---
   at Tests.Helpers.Database.OpenAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\Database.cs:line 19
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 19
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

```

### Sales with insufficient stock should return error and change nothing

**Message:**

```
Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
```

**StackTrace:**

```
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Helpers.Database.OpenAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\Database.cs:line 19
   at Tests.Spectests.OrdersInventoryContractTests.Sales_InsufficientStock_FailsAndNoChange() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 95
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

```

### Create product via service → row exists in DB

**Message:**

```
Expected res.IsSuccessStatusCode to be true, but found False.
```

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

### Create warehouse via service → row exists in DB

**Message:**

```
Expected res.IsSuccessStatusCode to be true, but found False.
```

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

### Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec

**Message:**

```
Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
```

**StackTrace:**

```
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.WaitForPendingOpen()
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 100
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

```

### Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized

**Message:**

```
Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
```

**StackTrace:**

```
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 83
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

```

## Console output (tail)

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  Determining projects to restore...
  All projects are up-to-date for restore.
C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\DbUtility.cs(21,12): warning CS8603: Possible null reference return. [C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\Test-First.csproj]
C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs(94,9): warning CS8604: Possible null reference argument for parameter 'source' in 'PublicUserDto Enumerable.First<PublicUserDto>(IEnumerable<PublicUserDto> source)'. [C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\Test-First.csproj]
  Test-First -> C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll
Test run for C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\bin\Debug\net8.0\Test-First.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.13.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
dotnet.exe : [xUnit.net 00:00:00.14]     Create warehouse via service ΓåÆ row exists in DB [FAIL]
At C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Scripts\Tests.ps1:76 char:15
+ $testOutput = & dotnet @argList 2>&1
+               ~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: ([xUnit.net 00:0...ts in DB [FAIL]:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
[xUnit.net 00:00:00.14]     Create product via service ΓåÆ row exists in DB [FAIL]
  Failed Create warehouse via service ΓåÆ row exists in DB [54 ms]
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
[xUnit.net 00:00:15.12]     Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec [FAIL]
[xUnit.net 00:00:15.12]     Sales: stock decreases per item and one log row per item [FAIL]
  Failed Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec [14 s]
  Error Message:
   Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
  Stack Trace:
     at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.WaitForPendingOpen()
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Roles_CRUD_and_conflicts_behave_as_spec() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 100
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

  Failed Sales: stock decreases per item and one log row per item [15 s]
  Error Message:
   Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
  Stack Trace:
     at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, SqlCommand command, Boolean callerHasConnectionLock, Boolean asyncClose)
   at Microsoft.Data.SqlClient.TdsParser.Connect(ServerInfo serverInfo, SqlInternalConnectionTds connHandler, TimeoutTimer timeout, SqlConnectionString connectionOptions, Boolean withFailover)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.AttemptOneLogin(ServerInfo serverInfo, String newPassword, SecureString newSecurePassword, TimeoutTimer timeout, Boolean withFailover)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.LoginNoFailover(ServerInfo serverInfo, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance, SqlConnectionString connectionOptions, SqlCredential credential, TimeoutTimer timeout)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds.OpenLoginEnlist(TimeoutTimer timeout, SqlConnectionString connectionOptions, SqlCredential credential, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance)
   at Microsoft.Data.SqlClient.SqlInternalConnectionTds..ctor(DbConnectionPoolIdentity identity, SqlConnectionString connectionOptions, SqlCredential credential, Object providerInfo, String newPassword, SecureString newSecurePassword, Boolean redirectedUserInstance, SqlConnectionString userConnectionOptions, SessionData reconnectSessionData, Boolean applyTransientFaultHandling, String accessToken, DbConnectionPool pool, Func`3 accessTokenCallback)
   at Microsoft.Data.SqlClient.SqlConnectionFactory.CreateConnection(DbConnectionOptions options, DbConnectionPoolKey poolKey, Object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningConnection, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.CreatePooledConnection(DbConnectionPool pool, DbConnection owningObject, DbConnectionOptions options, DbConnectionPoolKey poolKey, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionPool.CreateObject(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.UserCreateRequest(DbConnection owningObject, DbConnectionOptions userOptions, DbConnectionInternal oldConnection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.WaitForPendingOpen()
--- End of stack trace from previous location ---
   at Tests.Helpers.Database.OpenAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\Database.cs:line 19
   at Tests.Spectests.OrdersInventoryContractTests.Sales_DecreasesStock_And_WritesLogs() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 19
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

[xUnit.net 00:00:15.12]     Sales with insufficient stock should return error and change nothing [FAIL]
[xUnit.net 00:00:15.12]     Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized [FAIL]
[xUnit.net 00:00:15.12]     Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info [FAIL]
  Failed Sales with insufficient stock should return error and change nothing [1 ms]
  Error Message:
   Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
  Stack Trace:
     at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Helpers.Database.OpenAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Helpers\Database.cs:line 19
   at Tests.Spectests.OrdersInventoryContractTests.Sales_InsufficientStock_FailsAndNoChange() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\OrdersInventoryContractTests.cs:line 95
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

  Failed Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized [< 1 ms]
  Error Message:
   Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
  Stack Trace:
     at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Users_get_list_excludes_password_and_is_authorized() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 83
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

  Failed Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info [< 1 ms]
  Error Message:
   Microsoft.Data.SqlClient.SqlException : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
---- System.ComponentModel.Win32Exception : The system cannot find the file specified.
  Stack Trace:
     at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, UInt32 waitForMultipleObjectsTimeout, Boolean allowCreate, Boolean onlyOneCheckConnection, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionPool.TryGetConnection(DbConnection owningObject, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at Microsoft.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at Microsoft.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry, SqlConnectionOverrides overrides)
   at Microsoft.Data.SqlClient.SqlConnection.InternalOpenAsync(CancellationToken cancellationToken)
--- End of stack trace from previous location ---
   at Tests.Spectests.AuthContractTests.SeedAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 24
   at Tests.Spectests.AuthContractTests.LoginAsync() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 40
   at Tests.Spectests.AuthContractTests.Login_returns_token_and_user_info() in C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\tests\src\Spectests\AuthContractTests.cs:line 76
--- End of stack trace from previous location ---
----- Inner Stack Trace -----

Results File: C:\Users\Saphy\Desktop\Specialisterne Academy\Gruppeprojekt\Lagerstyrings-System\Reports\2025-10-23_11-56-43\TestResults.trx

Failed!  - Failed:     7, Passed:     1, Skipped:     0, Total:     8, Duration: 15 s - Test-First.dll (net8.0)
```
