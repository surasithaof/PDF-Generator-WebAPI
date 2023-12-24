FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PDF.Generator.WebAPI/PDF.Generator.WebAPI.csproj", "PDF.Generator.WebAPI/"]
RUN dotnet restore "PDF.Generator.WebAPI/PDF.Generator.WebAPI.csproj"
COPY . .
WORKDIR "/src/PDF.Generator.WebAPI"
RUN dotnet build "PDF.Generator.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PDF.Generator.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#####################
#PUPPETEER RECIPE
# ref: https://github.com/hardkoded/puppeteer-sharp/issues/1180
#####################
# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# installs, work.
RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    && apt-get update \
    && apt-get install -y google-chrome-unstable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf \
      --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*
#####################
#END PUPPETEER RECIPE
#####################
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/google-chrome-unstable"

ENTRYPOINT ["dotnet", "PDF.Generator.WebAPI.dll"]
