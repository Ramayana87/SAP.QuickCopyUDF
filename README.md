# SAP.QuickCopyUDF
Tool copy UDF, UDT, UDO, data ...

## Connection Method
This application connects to SAP Business One using **DI Service (Service Layer)** via REST API.

### Technical Details:
- **Connection Type**: DI Service (Service Layer) - NOT DI API
- **Protocol**: HTTPS REST API
- **Endpoints**: `/b1s/v1/` (Service Layer endpoints)
- **Authentication**: Session-based using B1SESSION cookie
- **Supported Operations**:
  - UserTablesMD (User-Defined Tables)
  - UserFieldsMD (User-Defined Fields)
  - UserObjectsMD (User-Defined Objects)

### Note about SAPbobsCOM Reference:
The `SAPbobsCOM.dll` is referenced in this project, but it is **ONLY** used for enum types (such as `BoFieldTypes`, `BoFldSubTypes`, `BoYesNoEnum`, etc.). The application does NOT use DI API for connecting to or interacting with SAP Business One. All operations are performed through the Service Layer REST API.
