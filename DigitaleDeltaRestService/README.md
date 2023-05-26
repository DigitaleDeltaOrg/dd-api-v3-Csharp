# Introduction

This root-document is written in English. All documents other documents, will be written in Dutch.

The DigitaleDelta API V3 (DD-API V3) is a universal 
[API](https://nl.wikipedia.org/wiki/Application_programming_interface) for 
requesting all kinds measurement-related data for the Dutch aquatic domain.
The goal is to uniform data and a simple method to request data for different kinds of audiences.
The requested data can be exported in several formats.
A further goal is to query across several data sources, sharing the same API.

## Why

Currently, the [Digitale Delta]() comprises of a several 
[APIs](https://nl.wikipedia.org/wiki/Application_programming_interface) serving specific goals. 
Furthermore, not all implementations of those APIs are implemented equally. 
Also the data sources they are serving, are not all implemented according to the Dutch [Aquo](https://aquo.nl) standard.
This leads to ambiguous data, which makes combining data unreliable.
This [API](https://nl.wikipedia.org/wiki/Application_programming_interface) tries to solve these issues.

## How

We combine [OData](https://www.odata.org) (a search standard) with OMS (exchange standard) and standard definition 
sources (i.e. [Aquo](https://aquo.nl)).

## OData

[OData](https://www.odata.org) is an ISO certified OASIS standard for requesting data using 
[REST API](https://en.wikipedia.org/wiki/Representational_state_transfer)s.
For it to work well, OData requires a data model specified in the
[EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) language. 
This can be either specified in [XML](https://www.w3.org/XML/)- or in [JSON](https://www.json.org/json-en.html).
It describes entities, their properties and their relations.
Normally the [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) is generated from the 
data source (database). 
In this case, the  [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) is crafted 
manually, because DD-API V3 dus not define or describe the underlying storage model. 
model.
By defining the [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) in the standard, 
we define a *virtual* data model, based on the Dutch OMS profile.

OData is capable of working with geographical (or geometrical) queries. The queries are expressed in 
[WellKnownText (WKT)](https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry)
because this fits well with REST architecture. [GeoJSON](https://datatracker.ietf.org/doc/html/rfc7946) 
is much harder to use in [REST API](https://en.wikipedia.org/wiki/Representational_state_transfer)s.
Export of geo-information, however, *is* expressed in [GeoJSON](https://datatracker.ietf.org/doc/html/rfc7946) format.

## OMS

OMS (Observations, Measurements & Sampling, or O&M:2022) is an 
[OGC](https://www.ogc.org)/[ISO](https://www.iso.org/home.html)-standard for exchanging measurement data,
the 2022-edition of [Observations & Measurements](https://www.ogc.org/standards/om).
We'll try to change as little as possible to the standard OMS-format.
OMS supports all measurement types we work with in the aquatic domain:

- quantity (timeseries, timeseriesML)
- grids (NetCDF)
- coverages (CoverageJSON)
- quality (field- and laboratory measurements)

### OMS profile

OMS is huge and flexible, this means multi-interpretable. The DD-API V3 is designed, however, to share the 
*same definition and meaning*. Therefore, we'll create a Dutch aqua-profile for OMS. This will result in a clear, 
concise definition for measurements and related data for the Dutch aquatic domain.
This will make it possible to combine data from different sources, without ambiguity.
And in turn, this will make it easier to exchange data with, for example, INSPIRE.

## OData & OMS

Unfortunately, OData and OMS do not work together smoothly.
For instance: OMS uses a dictionary for properties Parameter and Metadata. 
While the underlying definition of OData (the CSDL) does offer
a Dictionary keyword, OData does not recognize it. 
To solve that, the OData definition is defined in code, for which the 
framework generates an OData definition in the /$metadata endpoint. This can be used to compare to the standard OData 
definition.

## Reference system

DD-API V3 uses an internal [reference](reference.md) system.
All entities known to the system *must* be findable using the /v3/odata/reference endpoint.
All references in use in observations *must* be present in the reference system.
There can be several sources of reference types:

- [TaxaInfo](https://taxainfo.nl)
- [Aquo](https://aquo.nl)
- Organisation-specific

TaxaInfo is *the* source for biological taxa.
Aquo is *the* source for chemical-, physical- and other water related parameters, quantities, qualifications, units, 
sampling/value-determination methods, etc. 

References for parameter should adhere to above definitions as closely as possible.

Organisation-specific references might expand on Aquo, but if the definition is part of Aquo, the Aquo definition must 
be used.

An example: there are more sampling methods in use than Aquo describes.
It is *advisable* to request such methods to be added to Aquo.

### Why a reference system

The goal of [JSON](https://www.json.org/json-en.html) as a data format, is to make it readable to a human. 
Entities that only have a meaningless identifier, i.e. a UUID as [Aquo](https://aquo.nl) uses, 
it is no longer human readable.
The reference system provides a bridge between human readable entities and hard-to-interpret Ids.

## How it works

DD-API V3 is a [REST API](https://en.wikipedia.org/wiki/Representational_state_transfer). 
It consists of several layers, with some implementation-specific.

### EDMX/Open API

The [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) and
the [Open API-specification](https://www.openapis.org/) 
are the base elements of the [API](https://nl.wikipedia.org/wiki/Application_programming_interface). 
An [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) is written in
CSDL: [Common Schema Definition Language](https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html).

These components are (mostly) platform-independent.

### OData Parsers

OData parsers transform the OData queries into a programming structure that can be used to interpret the user's query, 
usually in the form of an [Abstract Syntax Tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree) or AST.
Many popular programming languages and frameworks have tools that can deal with OData queries and ASTs.

### Query/Data layer

The query/data layer translates the ASTs to queries that the underlying storage model understands. This will be 
implementation-specific.

## Parameters

A parameter can be (almost) any lookup value (observed property).
Exceptions are made for:

- foi (location)

## Metadata

Metadata will be any non-observed value that holds a non-lookup value. Examples:
- Magnification factor
- Sample code


TODO:
Remove, or place in a different position...

##### Polymorphic (de)serialization: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
##### Required: [JsonRequired]: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties

## GeoJSON encoding
https://github.com/opengeospatial/omsf-profile/blob/master/omsf-json/examples/measure-observation_geojson_feature-collection.json


/references
    id: string (uuid)
    type: string
    organisation: /references/type eq 'organisation'
    code: string
    parametertype: string
    cas: string

/observations
    type: string (oneof)
    phenomenontime: datetime
    validtime: datetime
    resulttime: datetime
    foi/code: /references/type eq 'measurementobject'
    foi/geometry: /references/type eq 'measurementobject'
    truth: bool
    uom/code: /references/type eq 'uom'
    uom/name: /references/type eq 'uom'
    measure: double
    count: int
    parameter/ANY(d:d/type eq ''): string (reference type)
    parameter/ANY(d:d/code eq ''): string
    parameter/ANY(d:d/taxontype eq ''): string
    parameter/ANY(d:d/taxongroup eq ''): string
    parameter/ANY(d:d/cas eq ''): string
    parameter/ANY(d:d/organisation eq ''): string

https://devblogs.microsoft.com/odata/customizing-filter-for-spatial-data-in-asp-net-core-odata-8/
