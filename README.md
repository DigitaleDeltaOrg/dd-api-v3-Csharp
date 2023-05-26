# dd-api-v3-Csharp

This repository contains the released code of the Proof of Concept of the [DD-API V3](https://github.com/DigitaleDeltaOrg/dd-api-v3).
This implementation uses a **simple** database structure based on a PostgreSQL database. *Consider a different data structure for production purposes.*.

The code shows how to separate the OData definition from the database layer by parsing the OData Abstract Syntax Tree and 
translating the calls to PostgreSQL database calls and translate the database's responses back to the OData model.


