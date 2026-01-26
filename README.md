# SAP.QuickCopyUDF
Tool copy UDF, UDT, UDO, data ...

## Features
- Copy User Defined Tables (UDT) between SAP B1 databases
- Copy User Defined Fields (UDF) between SAP B1 databases  
- Copy User Defined Objects (UDO) between SAP B1 databases
- Copy data between databases

## Connection Methods
The tool now supports two connection methods to the target SAP B1 database:

### HANA Service Layer (REST API)
- Uses SAP B1 Service Layer REST API
- Requires Service Address (e.g., `server:50000`)
- Supports HTTPS connections
- Recommended for cloud and modern deployments

### DI API Service (COM API)
- Uses SAP Business One DI API (COM-based)
- Requires SAP B1 HANA Server address
- Direct database connection via SAPbobsCOM
- Recommended for local/on-premise deployments with full DI API access

## Usage
1. Configure source database connection (SQL Server or HANA)
2. Select target service type (HANA Service Layer or DI API Service)
3. Configure target connection details
4. Click Login to connect
5. Use the Create buttons to copy UDT, UDF, or UDO objects
