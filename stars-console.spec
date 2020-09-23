
%define debug_package %{nil}
%define __jar_repack  %{nil}

Name:           stars
Url:            https://github.com/Terradue/Stars
License:        AGPLv3
Version:        %{_version}
Release:        %{_release}
Summary:        Stars Console Utility
BuildArch:      noarch
Source:         /usr/bin/stars
Requires:       dotnet-runtime-3.1
AutoReqProv:    no

%description
Generic Stars Console

%prep

%build


%install
cp -r %{_sourcedir}/* %{buildroot}
rm -f %{buildroot}/stars

%post

%postun


%clean
# rm -rf %{buildroot}


%files
/usr/lib/opensearch-client/*
/usr/bin/opensearch-client


%changelog
