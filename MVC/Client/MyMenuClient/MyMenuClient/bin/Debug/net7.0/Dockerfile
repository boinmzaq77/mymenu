FROM mcr.microsoft.com/dotnet/aspnet
RUN apt-get update
RUN apt-get install -y apt-utils
RUN apt-get install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
WORKDIR /mymenuclient
COPY . .

ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
 
ENTRYPOINT ["dotnet", "MyMenuClient.dll"]
