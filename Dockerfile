FROM mcr.microsoft.com/dotnet/core/runtime:3.1

# MAINTAINER Emmanuel Mathot <emmanuel.mathot@terradue.com>

ARG STARS_CONSOLE_TGZ
COPY $STARS_CONSOLE_TGZ /tmp/$STARS_CONSOLE_TGZ
RUN cd / && tar xvzf /tmp/$STARS_CONSOLE_TGZ