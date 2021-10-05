<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:verb="http://informatik.hu-berlin.de/xmlverbatim"
                exclude-result-prefixes="verb">
	<xsl:output method="html" indent="yes" encoding="ISO-8859-1"/>

	<xsl:param name="mediumLabel" select="/Dimap_Document/Product_Information/Delivery_Identification/Order_Identification/COMMERCIAL_REFERENCE"/>

	<!-- For internal order display -->
	<xsl:param name="indent-elements" select="false()" />

	<xsl:template match="/">
		<xsl:call-template name="HTML"/>
	</xsl:template>

	<xsl:template name="HTML">
		<html>
			<xsl:call-template name="HEAD"/>
			<xsl:call-template name="BODY"/>
		</html>
	</xsl:template>

	<xsl:template name="HEAD">
		<head>
			<xsl:apply-templates select="Dimap_Document" mode="headTitle"/>
			<xsl:call-template name="javascript"/>
			<xsl:call-template name="STYLE"/>
		</head>
	</xsl:template>

	<xsl:template name="STYLE">
   	<style type="text/css">
			<![CDATA[
      	a.open	{text-decoration: none;}
      	a.close	{text-decoration: none;}
			
      	fieldset.invisible fieldset.infs {display: none;}
      	fieldset.invisible dl.infs       {display: none;}
      	fieldset.invisible table.infs    {display: none;}
      	fieldset.invisible ul.infs    	{display: none;}
      	fieldset.invisible div.infs    	{display: none;}
      	fieldset.invisible a.open    		{display: inline;}
      	fieldset.invisible a.close    	{display: none;}

      	fieldset.visible fieldset.infs	{display: block;}
      	fieldset.visible dl.infs			{display: block;}
      	fieldset.visible table.infs		{display: table;}
      	fieldset.visible ul.infs			{display: block;}
      	fieldset.visible div.infs			{display: block;}
      	fieldset.visible a.open				{display: none;}
      	fieldset.visible a.close			{display: inline;}

 			]]>
   	</style>
	</xsl:template>

	<xsl:template name="javascript">
		<script type="text/javascript">
		
         function returnObjById( id )
		    {
		    if (document.getElementById)
		       var returnVar = document.getElementById(id);
		    else if (document.all)
			   var returnVar = document.all[id];
		    else if (document.layers)
			   var returnVar = document.layers[id];
		    return returnVar;
		    }

         function showHide( id )
         {
		    theObject=returnObjById(id);
		    if (theObject)
			   {
			   if (theObject.className == 'invisible')
			      {
			      theObject.className="visible";
			      }
			   else
			      {
			      theObject.className="invisible";
			      }
			   }
         }
			
         function loadSpotVolXmlInDiv( id , spotVolXml)
         {
    		var DIVElement = document.getElementById(id);
			alert(spotVolXml);
			
   		 if (DIVElement)
   		 {
         	showHide( id );

      		while (DIVElement.getElementsByTagName("DIV")[0])
      		{
         		DIVElement.removeChild(DIVElement.getElementsByTagName("DIV")[0]);
      		}
				
				DIVElement.innerHTML = '<iframe src='+spotVolXml+'/>';
      	}
			
         }
		</script>   
	</xsl:template>

	<xsl:template name="BODY">
		<body>
			<xsl:apply-templates select="Dimap_Document" mode="bodyTitle"/>
			<xsl:apply-templates select="Dimap_Document" mode="prodList"/>
		</body>
		<!--<xsl:call-template name="SOURCE"/> -->
	</xsl:template>
	
	<xsl:template match="Dimap_Document" mode="headTitle">
		<title><xsl:value-of select="Product_Information/Delivery_Identification/Order_Identification/COMMERCIAL_REFERENCE"/></title>
	</xsl:template>

	<xsl:template match="Dimap_Document" mode="bodyTitle">
		<h1><xsl:value-of select="Dataset_Identification/DATASET_NAME"/><xsl:text> </xsl:text><xsl:value-of select="Dataset_Identification/DATASET_INDEX"/></h1>
		<h1>Order Number : <xsl:value-of select="Product_Information/Delivery_Identification/Order_Identification/COMMERCIAL_REFERENCE"/></h1>
		<h2>Production Date : <xsl:value-of select="Product_Information/Delivery_Identification/PRODUCTION_DATE"/></h2>
	</xsl:template>

	<xsl:template match="Dimap_Document" mode="prodList">
		<xsl:apply-templates select="Dataset_Content/Dataset_Components/Component" mode="prodList"/>
	</xsl:template>

	<xsl:template match="Component" mode="prodList">
		<xsl:variable name="spotProdDir" select="substring-before(COMPONENT_PATH/@href,'/')" />
		<xsl:variable name="spotProdXml" select="document(COMPONENT_PATH/@href)" />
		<dt><a href="{COMPONENT_PATH/@href}" target="_self"><xsl:value-of select="COMPONENT_TITLE"/></a></dt>
		<!--
		<fieldset id="{COMPONENT_TITLE}" class="invisible" width="100%" height="100%">
         <legend>
				<a class="open" href="#{COMPONENT_TITLE}" onclick="javascript:showHide('{COMPONENT_TITLE}');"><xsl:value-of select="COMPONENT_TITLE"/> +</a>
				<a class="close" href="#{COMPONENT_TITLE}" onclick="javascript:showHide('{COMPONENT_TITLE}');"><xsl:value-of select="COMPONENT_TITLE"/> -</a></legend>
			<div class="infs" width="100%" height="100%">
				<iframe src="{COMPONENT_PATH/@href}" width="100%" height="100%" scrolling="auto"/>
			</div>
		</fieldset>
		-->
	</xsl:template>
</xsl:stylesheet>

