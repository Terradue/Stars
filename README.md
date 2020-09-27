<h1 align="center">Stars</h1>


<h2 align="center">
Spatio Temporal Asset Router Services
<br/>
<img src="docs/logo/Stars_logo.png" height="200" />

</h2>

<h3 align="center">

  <!-- [![Build Status](https://travis-ci.com/Terradue/DotNetStac.svg?branch=develop)](https://travis-ci.com/Terradue/DotNetStac) -->
  [![NuGet](https://img.shields.io/nuget/vpre/Terradue.Stars.Services)](https://www.nuget.org/packages/Terradue.Stars.Services/)
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

**Stars** is a set of services for working with Spatio Temporal Catalog such as [STAC](https://stacspec.org) but not only.

All [services are built around the **Catalog**](#Services) :

* **Router** is a service for navigating a catalog.
* **Supplier** service enables Data Provider for the *assets* of the catalog. E.g. Copernicus datasets providers like [DIAS](https://www.copernicus.eu/en/access-data/dias) can be implemented as a supllier and managed in this service.
* **Harvester** allows the assets to be processed by various modules for extracting additional information.
* **Processing** the *assets* to enhance their availibility.
* **Coordinator** for linking the different resources in a catalog. For instance, by gathering items in a collection.

***

## Services

**Stars** is basically a collection of services implemented in .Net that can be used to implement command line tools, web services or any programmtic logic arounf Spatio Temporal Catalogs.
They can be combined togheter to perform simples operations like listing a catalog to complex processing of assets.

<IMG SRC="docs/diagrams/servicesStarsConcepts.svg" ALIGN=”left” HSPACE=”50” VSPACE=”50” height="400"/> 

### Router

 This is a recursive function for trigger functions during the navigation of a Catalog. Basically it reads a catalog as a tree and crawl in every node of the catalog allowing the programmer to set functions to be executed when it meets a new node, before and after branching to the node children or when the parser comes to a leaf node.

 ### 