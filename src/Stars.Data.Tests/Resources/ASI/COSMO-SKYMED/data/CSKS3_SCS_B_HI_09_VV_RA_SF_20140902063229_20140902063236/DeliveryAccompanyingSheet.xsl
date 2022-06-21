<?xml version="1.0" encoding="UTF-8"?>
<!-- edited with XMLSPY v5 rel. 4 U (http://www.xmlspy.com) by User (User) -->
<?xmlspysamplexml D:\GENERAZIONE XSLT-FO per DFCON\AccompanySheetTest.xml?>
<?xmlspyxslfo D:\GENERAZIONE XSLT-FO per DFCON\AccompanySheetTest.xml?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">
	<xsl:output method="xml"/>
	<!-- ===================== -->
	<!-- MAIN SECTION OF LAYOUT -->
	<!-- ===================== -->
	<xsl:template match="/">
		<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
			<fo:layout-master-set>
				<fo:simple-page-master master-name="A4" page-height="29.7cm" page-width="21cm" margin-top="2cm" margin-bottom="1cm" margin-left="2cm" margin-right="2cm" initial-page-number="1">
					<fo:region-before extent="1cm"/>
					<fo:region-body margin-top="1cm" margin-bottom="1.4cm"/>
					<fo:region-after extent="1cm"/>					
				</fo:simple-page-master>
			</fo:layout-master-set>
			<fo:page-sequence master-reference="A4">
			
			     <!-- Classification Level -->
			     <fo:static-content flow-name="xsl-region-before">					
					<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
						<fo:table-column/>
						<fo:table-body>						    
					        <fo:table-row>
							    <fo:table-cell>
								   <fo:block text-align="center" font-family="Times" font-size="10pt" font-weight="bold"> <xsl:value-of select="/DeliveryAccompanyingSheet/ClassLevel"/> </fo:block>
							    </fo:table-cell>
						     </fo:table-row>
						</fo:table-body>
					</fo:table>					
				</fo:static-content>
				
				<fo:static-content flow-name="xsl-region-after">					
					<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
						<fo:table-column/>
						<fo:table-body>						    
					        <fo:table-row>
							    <fo:table-cell>
								   <fo:block text-align="center" font-family="Times" font-size="10pt" font-weight="bold"> <xsl:value-of select="/DeliveryAccompanyingSheet/ClassLevel"/> </fo:block>
							    </fo:table-cell>
						     </fo:table-row>
						</fo:table-body>
					</fo:table>
					
					<fo:block text-align="end" font-size="8pt">Page <fo:page-number/>
					</fo:block>					
				</fo:static-content>				
				<!-- Classification Level -->
				
				<fo:flow flow-name="xsl-region-body">
					<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
						<fo:table-column/>
						<fo:table-body>
							<fo:table-row>
								<fo:table-cell>
									<fo:block text-align="center" font-family="Times" font-size="18pt" font-weight="bold">COSMO-SkyMed – Accompany Sheet</fo:block>
								</fo:table-cell>
							</fo:table-row>
							<fo:table-row>
								<fo:table-cell height="18pt">
									<fo:block/>
								</fo:table-cell>
							</fo:table-row>
							<xsl:apply-templates/>
						</fo:table-body>
					</fo:table>
				</fo:flow>
			</fo:page-sequence>
		</fo:root>
	</xsl:template>
	<!-- ================ -->
	<!-- PRODUCT SECTION -->
	<!-- ================ -->
	<xsl:template match="Product">
		<fo:table-row>
			<fo:table-cell height="18pt">
				<fo:block/>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell>
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
					<fo:table-column column-width="6.5cm"/>
					<fo:table-column/>
					<fo:table-body>
						<xsl:apply-templates/>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ================ -->
	<!-- PRODUCT SECTION -->
	<!-- ================ -->
	<xsl:template match="Request">
		<fo:table-row>
			<fo:table-cell>
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
					<fo:table-column column-width="6.5cm"/>
					<fo:table-column/>
					<fo:table-body>
						<xsl:apply-templates/>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ================ -->
	<!-- DELIVERY TO SECTION-->
	<!-- ================ -->
	<xsl:template match="DeliveryTo">
		<fo:table-row>
			<fo:table-cell>
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">RECEIVER ADDRESS</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell>
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="1">
					<fo:table-column column-width="6.5cm"/>
					<fo:table-column/>
					<fo:table-body>
						<xsl:apply-templates/>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell>
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">PRODUCTS</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ================ -->
	<!-- RECEIVER NAME       -->
	<!-- ================ -->
	<xsl:template match="RequestName">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">RECEIVER NAME</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../DeliveryTo/Surname"/><xsl:text>&#32;</xsl:text><xsl:value-of select="../../DeliveryTo/Name"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- DELIVERY REQUEST ID -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">DELIVERY REQUEST ID</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../DeliveryRequestId"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- DELIVERY DATE & TIME -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">DELIVERY DATE &amp; TIME</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../DeliveryDateUTC"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- MAIL TYPE  -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">MAIL TYPE </fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../DeliveryTo/MailType"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- USER ID   -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">USER ID</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../RequestorUserId"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- USER REQUEST ID   -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">USER REQUEST ID</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../RequestId"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- USER REQUEST NAME -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">SERVICE NAME</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../ServiceRequestName"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- SERVICE NAME -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">SERVICE NAME</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../RequestName"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>

		<!-- ==================== -->
		<!-- NOTE -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">COMMENT</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../Note"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>

	</xsl:template>
	<!-- ================= -->
	<!-- PRODUCT FILENAME -->
	<!-- ================= -->
	<xsl:template match="ProductTags/ProductFileName">
		<!-- ============== -->
		<!-- PRODUCT NAME-->
		<!-- ============== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">PRODUCT NAME</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en"><xsl:value-of select="."/></fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============== -->
		<!-- PRODUCT ID -->
		<!-- ============== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">PRODUCT ID</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" font-weight="bold" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../ProductId"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============== -->
		<!-- medium description -->
		<!-- ============== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-weight="bold" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					MEDIUM/MEDIA/COMPRESSED FILE LABEL
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="0">
					<fo:table-column/>
					<fo:table-body>
						<xsl:for-each select="../../Medium">
							<fo:table-row>
								<fo:table-cell border-style="solid">
									<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
										<xsl:value-of select="Label"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</xsl:for-each>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-weight="bold" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					MEDIUM/MEDIA/ COMPRESSED FILE CREATION DATE
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="0">
					<fo:table-column/>
					<fo:table-body>
						<xsl:for-each select="../../Medium">
							<fo:table-row>
								<fo:table-cell border-style="solid">
									<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
										<xsl:value-of select="CreationDate"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</xsl:for-each>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-weight="bold" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					COPY NUMBER
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="0">
					<fo:table-column/>
					<fo:table-body>
						<xsl:for-each select="../../Medium">
							<fo:table-row>
								<fo:table-cell border-style="solid">
									<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
										<xsl:value-of select="CopyNumberOf"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</xsl:for-each>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-weight="bold" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					OPERATOR NOTE
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:table width="100%" table-layout="fixed" space-before.optimum="1pt" space-after.optimum="2pt" border="0">
					<fo:table-column/>
					<fo:table-body>
						<xsl:for-each select="../../Medium">
							<fo:table-row>
								<fo:table-cell border-style="solid">
									<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
										<xsl:value-of select="OperatorNote"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</xsl:for-each>
					</fo:table-body>
				</fo:table>
			</fo:table-cell>
		</fo:table-row>

		<!-- ==================== -->
		<!-- LINUX COMMAND FOR PRODUCT REBUILD  -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					LINUX COMMAND FOR PRODUCT REBUILD
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../SplittedProductRebuildInstructions"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- PRODUCTTYPE  -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					PRODUCT TYPE
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../ProductType"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ==================== -->
		<!-- PRODUCT FORMAT  -->
		<!-- ==================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					PRODUCT FORMAT  
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../ProductFormat "/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================ -->
		<!-- PRODUCT CLASSIFICATION LEVEL  -->
		<!-- ============================  -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					CLASSIFICATION LEVEL
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../../ClassificationLevel"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--PRODUCT SPECIFICATION DOCUMENT  -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					PRODUCT SPECIFICATION DOCUMENT
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../ProductSpecificationDocument"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--SAMPLE FORMAT   -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					SAMPLE FORMAT
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../SampleFormat"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--SAMPLES PER PIXEL  -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					SAMPLES PER PIXEL
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../SamplesPerPixel"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--BITS PER SAMPLE  -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					BITS PER SAMPLE
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../BitsPerSample"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--PROCESSING CENTRE   -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					PROCESSING CENTRE
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../ProcessingCentre"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
		<!-- ============================== -->
		<!--  PROVIDER ID   -->
		<!-- ============================== -->
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					PROVIDER ID
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="../ProviderId"/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- SURNAME-->
	<!-- ==================== -->
	<xsl:template match="Surname">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Surname
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- NAME-->
	<!-- ==================== -->
	<xsl:template match="Name">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Name
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- COMPANY-->
	<!-- ==================== -->
	<xsl:template match="Company">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Company
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- ADDRESS-->
	<!-- ==================== -->
	<xsl:template match="Address">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Address
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- ZIPCODE-->
	<!-- ==================== -->
	<xsl:template match="ZIPCode">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					ZIPCode
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- CITY-->
	<!-- ==================== -->
	<xsl:template match="City">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					City
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- STATE PROVINCE-->
	<!-- ==================== -->
	<xsl:template match="State/Province">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					StateProvince
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- COUNTRY-->
	<!-- ==================== -->
	<xsl:template match="Country">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Country
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- PHONE-->
	<!-- ==================== -->
	<xsl:template match="Phone">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Phone
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- MODE-->
	<!-- ==================== -->
	<xsl:template match="Mode">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Mode
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
	<!-- ==================== -->
	<!-- INVOICE NUMBER-->
	<!-- ==================== -->
	<xsl:template match="InvoiceNumber">
		<fo:table-row>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					Invoice Number
				</fo:block>
			</fo:table-cell>
			<fo:table-cell border-style="solid">
				<fo:block text-align="left" font-family="Times" font-size="10pt" space-before="10pt" space-after="10pt" wrap-option="wrap" overflow="auto" hyphenate="true" language="en">
					<xsl:value-of select="."/>
				</fo:block>
			</fo:table-cell>
		</fo:table-row>
	</xsl:template>
</xsl:stylesheet>
