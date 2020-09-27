<h1 align="center">Stars</h1>


<h2 align="center">
Spatio Temporal Asset Router Services
<br/>
<img src="docs/logo/Stars_logo_wide.png" height="200" />

</h2>

<h3 align="center">

  <!-- [![Build Status](https://travis-ci.com/Terradue/DotNetStac.svg?branch=develop)](https://travis-ci.com/Terradue/DotNetStac) -->
  [![NuGet](https://img.shields.io/nuget/vpre/Terradue.Stars.Services)](https://www.nuget.org/packages/Terradue.Stars.Services/)
  [![License](https://img.shields.io/badge/license-AGPL3-blue.svg)](LICENSE)
  <!-- [![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/Terradue/DotNetStac/develop?filepath=example.ipynb) -->

</h3>

<h3 align="center">
  <a href="#Services-Principles">Principles</a>
  <span> · </span>
  <a href="#Getting-Started">Getting started</a>
  <span> · </span>
  <a href="#Documentation">Documentation</a>
  <span> · </span>
  <a href="#Developing">Developing</a>
</h3>

**Stars** is a set of services for working with Spatio Temporal Catalog such as [STAC](https://stacspec.org) but not only.

All the services runs around the [principle](#Principles) of navigating inside a Catalog called "**Routing**". When routing in a catalog, the programmer can use other services

* **Supplying** the *assets* of the catalog. E.g. the Copernicus datasets from the [DIAS](https://www.copernicus.eu/en/access-data/dias).
* **Carrying** the resources from a source to a **Destination**. For instance downloading the *catalogs*, *items* and *assets* locally.
* **Translating** the catalogs from a *data model* or an *encoding* to another. *Atom feeds* from an *OpenSearch Catalog* can be transformed to *STAC JSON Catalog*.
* **Processing** the *assets* to enhance their availibility. Archive assets can be extracted and metadata *harvested* for associating useful information with the data.
* **Cataloging** the *Spatio Temporal Assets* into the standard specifications [STAC](https://stacspec.org)


***

## Services Principles

