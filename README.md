# ALMS setup (demo/development)

Ez a README file az egész rendszer indításával foglalkozik, de a git repository csak a backend-et tartalmazza

A többi repo ezeken a linkeken elérhető:

[Frontend Github](https://github.com/Kalamansanai/alms-client)<br>
[Raspberry Pi Github](https://github.com/Kalamansanai/plm-rpi)

### Az ALMS elindításához 4 dolgot kell telepítenünk/elindítanunk:
- Adatbázis
- Backend
- Frontend
- Raspberry Pi(ok)

Ezek mind Debian 22-n futnak

Az indítás sorrendje gyakorlatilag nem számít, azonban javasolt a fenti sorrendben elvégezni.

## Adatbázis indítása
Adatbázisként MariDB-server 10.6.18-at használunk, ami nem része a Debian telepítésnek

### MariaDB telepítése
```
sudo apt install mariadb-server-10.6
```

## Backend indítása
A backend .NET-ben íródott, így a `dotnet run` paranccsal tudjuk elindítani a forráskód helyén

A program .NET 6.0-es verziójára készült, ami nem része a Debian telepítéseknek

### .NET telepítése 

[Microsoft dokumentáció](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian)<br>
[Stackoverflow issue](https://stackoverflow.com/questions/73753672/a-fatal-error-occurred-the-folder-usr-share-dotnet-host-fxr-does-not-exist)

A telepítéshez először hozzá kell adnunk a Microsoft "signing key"-t a biztonságos kulcsokhoz, valamint hozzá kell adnunk a csomag repository-t

```
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

Az Ubuntu összekeveri a .NET telepítéseket a hivatalos SDK előtti időkről, így először ezeket le kell kezelni és el kell távolítani

```
sudo apt remove dotnet* aspnetcore* netstandard*
```

Ezután telepítjük az SDK-t, ami magába foglalja az összes Development és runtime eszközt is
```
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-6.0
```
Fontos, hogy a 6.0 verziót telepítsük és ne a 8.0-t, ami a Microsoft dokumentációban található

### Backend indítása
Ha letelepítettük a .NET keretet, akkor az `alms-ssp/src/Api` mappán belül a `dotnet run` paranccsal elindíthatjuk a backend-et

## Frontend indítása

A frontendet NPM 8.5.1 segítségével indítjuk el, ami nem része a Debian telepítésnek


### NPM telepítése, frontend build-elése

```
npm install
npm run build
```

Telepítés után az `npm run check` paranccsal leellenőrizhető, hogy a typescript megfelelően lett-e formatálva

### Frontend indítása

A frontend-nek két módja van:
- development: `npm run dev`
- preview/demo: `npm run preview` (ajánlott)

Mindkét parancsot a `*/alms-client` directory-ban lehet elindítani

**A frontend csak Chromium alapú webböngészőkön fut hiánytalanul, Firefox-on nem működik**

(Teljes leírásért lásd: [Frontend README](https://github.com/Kalamansanai/alms-client/blob/master/readme.md))

# Raspberry Pi indítása

A Raspberry Pi-ok headless Ubuntu-t használnak, megadott image alapján

## Image-k

### Image készítése működő PI-ból
Linuxon:

```
sudo dd bs=4M if=/dev/sde of=./MyImage.img
```

Az `if` (input file) után a mountolt SD kártya nevét kell írni, az `of` (output file) után pedig a létrehozandó image nevét (./név.img esetén a megnyitott directoryba fogja létrehozni)

**Az image készítés akár 10+ percet is igénybe vehet és 32GB szabad helyet foglal el a célmappában**

[További információk az image készítésről](https://raspberrytips.com/create-image-sd-card/)

### Image új SD kártyára másolása

Az image leendő helyének meghatározására és a rajta lévő partíciók unmount-olása és az image másolása az SD kártya behelyezése után:

```
sudo fdisk -l

sudo umount /dev/sdx*

sudo dd bs=1M if=MyImage.img of=/dev/sdx
```
A `/dev/sdx`-et az SD kártya helyével kell helyettesíteni, a `MyImage.img`-t pedig a létrehozott image nevével              

**A felmásolásd akár 10+ percet is igénybe vehet**

[További információk az image másolásról](https://raspberrypi.stackexchange.com/questions/931/how-do-i-install-an-os-image-onto-an-sd-card)

## Konfiguráció

A fő python program (`git-repo-name/software/main.py`) egy config file-ból kapja meg a backend IP-jét, a kamera tulajdonságait és még néhány információt. Ez a `git-repo-name/software/lib/config/rpi.config` file-ban található.

# Automatikus indítás
Minden program a készülék bekapcsolásánál indul el. Ezt .service file-ok teszik lehetővé.

A `/etc/systemd/system` direcotry tartalmazza az indításhoz szükséges utasításokat, ide kell bemásolni/létrehozni a név.service file-okat

**Ezt a directory-t csak `sudo`-val vagy root-ként lehet kezelni**

A létrehozott file-okat 644-es elérésűvé kell tenni

```
sudo chmod 644 filenév
```

`.service` file példa:

```
[Unit]
Description=Print Current Working Directory

[Service]
Environment=DOTNET_CLI_HOME=/temp
ExecStart=/usr/bin/dotnet run --project /home/tk/Documents/plm/plm-ssp/src/Api

[Install]
WantedBy=multi-user.target
```     