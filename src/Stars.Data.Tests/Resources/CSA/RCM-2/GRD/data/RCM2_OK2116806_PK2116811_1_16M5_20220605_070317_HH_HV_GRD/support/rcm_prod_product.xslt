<?xml version="1.0" encoding="UTF-8" ?>
<!--=========================================================================
Filename      : rcm_prod_product.xslt@@/main/5 ( Pitt: 36635 )
Project       : RCM-GS
Module        : PGS - Product Metadata Generator
Document Type : XSLT
Purpose       : Convert XML file to HTML |Name|Value| tables, recursively.

(C) Copyright 2016 MacDonald, Dettwiler and Associates Ltd. and/or its affiliates. All Rights Reserved.

Author        Date          Changes
E. Jenkins    2016-06-07    Initial release
=============================================================================-->

<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" encoding="utf-8"/>
 
    <!-- Match root. -->
    <xsl:template match="/*[node()]">
		<html>
			<head>
    			<!-- Use root name as title. -->
				<title><xsl:value-of select="name()"/></title>
    			<!-- Set up table and list styles. -->
                <style>
					ul {
						list-style-type:none;
					}
					table {
    					border-collapse: collapse;
    					border: 1px solid black;
					}
					td {
						vertical-align: top;
    					border: 1px solid black;
					}
				</style>
			</head>
			<body>
    			<!-- Display root name. -->
        		<h3><xsl:value-of select="name()"/></h3>

        		<!--  Display any root attributes as a list. -->
        		<xsl:if test="count(@*) > 0">
					<ul>
						<xsl:for-each select="attribute::*">
        					<xsl:apply-templates select="." mode="root-attr" />
						</xsl:for-each>
					</ul>
        		</xsl:if>

        		<!--  Root table. -->
				<table>
        			<!--  Handle child elements. -->
					<xsl:for-each select="child::*">
        				<xsl:apply-templates select="." mode="child" />
					</xsl:for-each>
				</table>
			</body>
		</html>
    </xsl:template>
 
    <!-- Child template. -->
    <xsl:template match="*" mode="child">
        <xsl:choose>
    		<!-- When no children, just add row. -->
            <xsl:when test="count(./child::*) = 0">
				<tr>
					<td>
    					<!-- Display name and any attributes. -->
						<xsl:value-of select="name()"/>
        				<xsl:if test="count(@*) > 0">
							<xsl:text> </xsl:text>
                			<xsl:apply-templates select="@*" mode="attr" />
        				</xsl:if>
					</td>
					<td>
    					<!-- Display value. -->
						<xsl:value-of select="text()"/>
					</td>
				</tr>
            </xsl:when>

    		<!-- When children, start row and add a table of children. -->
            <xsl:when test="count(./child::*) > 0">
				<tr>
					<td>
    					<!-- Display name and any attributes. -->
						<xsl:value-of select="name()"/>
        				<xsl:if test="count(@*) > 0">
							<xsl:text> </xsl:text>
                			<xsl:apply-templates select="@*" mode="attr" />
        				</xsl:if>
					</td>
					<td>
    					<!-- Build table for "value". -->
						<table>
							<xsl:for-each select="child::*">
        						<xsl:apply-templates select="." mode="child" />
							</xsl:for-each>
						</table>
					</td>
				</tr>
            </xsl:when>
        </xsl:choose>
    </xsl:template>
 
	<!-- Attribute template - just attr="value". -->
    <xsl:template match="@*" mode="attr">
        <xsl:value-of select="name()"/>="<xsl:value-of select="."/>"
    </xsl:template>
 
	<!-- Root attribute template - attr="value" as a list item. -->
    <xsl:template match="@*" mode="root-attr">
        <li><xsl:value-of select="name()"/>="<xsl:value-of select="."/>"</li>
    </xsl:template>
 
</xsl:stylesheet>
