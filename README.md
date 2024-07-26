# ALMS setup (demo/development)

## Backend indítása (csak Linux (Ubuntu))
A backend .NET-ben íródott, így a `[dotnet run]` paranccsal tudjuk elindítani a forráskód helyén (ahová git clone-al másolva lett)

A program .NET 6.0.424-es verziójára készült, ami nem része eredetileg a linux telepítéseknek.

### .NET telepítése

[Microsoft dokumentáció](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian)

A telepítéshez először hozzá kell adnunk a Microsoft "signing key"-t a biztonságos kulcsokhoz, valamint hozzá kell adnunk a csomag repository-t

```
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

Ezután telepítjük az SDK-t, ami magába foglalja az összes Development és runtime eszközt is
```
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```



## Frontend indítása
