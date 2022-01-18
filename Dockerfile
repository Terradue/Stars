FROM centos:8

# MAINTAINER Emmanuel Mathot <emmanuel.mathot@terradue.com>

RUN yum install -y epel-release unzip procps \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg \
    && yum clean all -y

# Install HDF5 tools
RUN dnf install 'dnf-command(config-manager)' -y \
    && dnf config-manager -y --set-enabled powertools \
    && yum update -y \
    && yum install -y hdf5 \
    && yum clean all -y

ARG STARS_RPM
COPY $STARS_RPM /tmp/$STARS_RPM
RUN yum localinstall -y /tmp/$STARS_RPM
