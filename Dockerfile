FROM centos:7

# MAINTAINER Emmanuel Mathot <emmanuel.mathot@terradue.com>

ARG STARS_RPM
COPY $STARS_RPM /tmp/$STARS_RPM
RUN yum localinstall -y /tmp/$STARS_RPM