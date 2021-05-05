<h1 align="center">Stars</h1>


<h2 align="center">
Spatio Temporal Asset Router Services
<br/>
<img src="docs/logo/Stars_logo.png" height="200" />

</h2>

<h3 align="center">

  <!-- [![Build Status](https://travis-ci.com/Terradue/DotNetStac.svg?branch=develop)](https://travis-ci.com/Terradue/DotNetStac) -->
  [![NuGet](https://img.shields.io/nuget/vpre/Terradue.Stars)](https://www.nuget.org/packages/Terradue.Stars)
  [![License](https://img.shields.io/badge/license-AGPL3-blue.svg)](LICENSE)
  <!-- [![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/Terradue/DotNetStac/develop?filepath=example.ipynb) -->

</h3>

<h3 align="center">
  <a href="#Services">Services</a>
  <span> · </span>
  <a href="#Getting-Started">Getting started</a>
  <span> · </span>
  <a href="#Documentation">Documentation</a>
  <span> · </span>
  <a href="#Developing">Developing</a>
</h3>

**Stars** is a set of .Net middleware for working with Spatio Temporal Catalog such as [STAC](https://stacspec.org) but not only.

All [services are built around the **Catalog Resource**](#catalog-resource) :

* **Route** service for navigating the catalog resource.
* **Supply** service enables Data Providers for the *assets* of the catalog.
* **Harvest** allows the assets to be processed by various modules for extracting additional information.
* **Process** enable *assets* enhancement.
* **Store** for linking the different resources in a catalog. For instance, by gathering items in a collection.

***

## Services

**Stars** is basically a collection of services implemented in .Net that can be used to implement command line tools, web services or any programmtic logic arounf Spatio Temporal Catalogs.
They can be combined togheter to perform simples operations like listing a catalog to complex processing of assets.

<h4 align="center">
<IMG SRC="docs/diagrams/servicesStarsConcepts.svg" ALIGN=”left” HSPACE=”50” VSPACE=”50” height="300"/>
</h4>

### Catalog Resource

### Router

This is a recursive function for trigger functions during the navigation of a Catalog. Basically it reads a catalog as a tree and crawl in every node of the catalog allowing the programmer to set functions to be executed when it meets a new node, before and after branching to the node children or when the parser comes to a leaf node.
This service uses the plugins manager to find the appropriate router for a catalog data model and encoding.

> :mag: plugins implementing routers may reader various catalog data model and encoding. [Stars Tools](#Stars-Tools) implements natively
> * Atom feed from an [OpenSearch](https://github.com/dewitt/opensearch) results
> * [STAC](https://stacspec.org) ([catalog](https://github.com/radiantearth/stac-spec/tree/master/catalog-spec), [collection](https://github.com/radiantearth/stac-spec/tree/master/collection-spec), [item](https://github.com/radiantearth/stac-spec/tree/master/catalog-spec))

### Supplier

Service managing a collection of suppliers for providing with the assets. From a resource description (e.g. uid, AOI, time...) it requests them for the data availability and organize the *delivery* of the assets using **Carriers** (e.g. HTTP/FTP/S3 download). It also allows to make **orders** to suppliers that offers offline datasets. Suppliers and Carriers in this service are managed as configurable plugins.

> :satellite: E.g. Copernicus datasets providers like [DIAS](https://www.copernicus.eu/en/access-data/dias) can be implemented as a supllier and managed in this service.

### Harvester

Collection of executors that perform a scan of the data to extract useful infromation that can be added to the catalog. Executors are managed as configurable plugins.

> :package: Harvester Service includes an archive extractor by default

### Processing

An abstracted service enabling a trigger for procesing *items* of a catalog.

### Coordinator

The coordination service links the catalogs and their items.

> :earth_africa: By default the coordinator creates a STAC catalog with the items and their assets.

## Getting Started

### Stars Command Line Tools

Stars Services are used to provide common operations with catalogs. The currently implemented ones are:

* **List** crawls the tree of an input catalog
* **Copy** replicates the tree of items and assets from the tree of an input catalog

## Documentation

Documentation will come soon

## Developing

To ensure development libraries are installed, restore all dependencies

```
> dotnet restore src
```
