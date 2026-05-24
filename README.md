# Paplašinātās realitātes (AR) Android lietotne digitālās mākslas izstāžu izveidei un apskatei

> Bakalaura darba ietvaros izstrādāta paplašinātās realitātes lietotne Android ierīcēm, kas ļauj lietotājam izveidot virtuālu digitālās mākslas izstādi, izvietojot 3D objektus fiziskā telpā, un ļauj skatītājam apskatīt izstādi caur mobilās ierīces kameru.

---

## Par lietotni

Lietotne izmanto **Google ARCore** bezmarķieru SLAM izsekošanas tehnoloģiju un **Unity AR Foundation** ietvaru. Lietotne darbojas divos režīmos:

| Režīms | Apraksts |
|--------|----------|
| **Izstādes veidotājs** | Lietotājs augšupielādē savus `.glb` formāta 3D modeļus, izvieto tos uz atpazītām plaknēm, mērogo, rotē un saglabā izveidoto izstādi |
| **Izstādes skatītājs** | Lietotājs ielādē saglabātu izstādi un apskata to caur mobilās ierīces kameru |

---

## Prasības

- Android ierīce ar **ARCore atbalstu** ([atbalstīto ierīču saraksts](https://developers.google.com/ar/devices))
- **Android 7.0** vai jaunāka versija

---

## Lietotnes lejupielāde

Gatavs APK fails ir pieejams **[Releases](../../releases)** sadaļā.

---

## Lietošana

### Izstādes veidotājs

1. Atver lietotni un izvēlies **Izstādes Veidotājs** režīmu
2. Piekrīti autortiesību noteikumiem
3. Augšupielādē `.glb` modeļus no ierīces atmiņas
4. Skenē telpu, lai atpazītu plaknes
5. Novieto **divus kalibrācijas atskaites punktus** uz grīdas atpazīstamās vietās telpā
6. Izvieto objektus uz plaknēm, mērogo un rotē tos
7. Saglabā izstādi

### Izstādes skatītājs

1. Izvēlies **Izstādes Skatītājs** režīmu
2. Ielādē izstādes failu
3. Novieto kalibrācijas atskaites punktus **tajās pašās vietās** kā veidotājs
4. Apskatī izstādi, staigājot apkārt objektiem

---

## Ieteicamie .glb modeļu ierobežojumi

| Parametrs | Ieteicamais ierobežojums |
|-----------|------------------------|
| Kopējais modeļu failu izmērs izstādē | ≤ 100 MB |
| Trīsstūru skaits uz modeli | ≤ 25 000 |
| Kopējais trīsstūru skaits scēnā | ≤ 250 000 |

---

## Autors

**Krišjānis Vītiņš**

Rīgas Tehniskā universitāte — Datorzinātnes, informācijas tehnoloģijas un enerģētikas fakultāte

---

Bachelor's thesis, 2026

# Augmented Reality (AR) Android app for creating and viewing digital art exhibitions

> As part of the bachelor's thesis, an augmented reality app for Android devices was developed that allows the user to create a virtual digital art exhibition by placing 3D objects in physical space, and allows the viewer to view the exhibition through the mobile device's camera.

---

## About the app

The app uses **Google ARCore** markerless SLAM tracking technology and the **Unity AR Foundation** framework. The app operates in two modes:

| Mode | Description |
|-------|-----------|
| **Exhibition creator** | The user uploads their `.glb` format 3D models, places them on recognized planes, scales and rotates them and saves created exhibition |
| **Exhibition viewer** | The user loads a saved exhibition and views it through the mobile device's camera |

---

## Requirements

- Android device with **ARCore support** ([List of supported devices](https://developers.google.com/ar/devices))
- **Android 7.0** or later

---

## Downloading the app

The finished APK file is available in the **[Releases](../../releases)** section.

---

## Usage

### Exhibition Creator

1. Open the app and select **Exhibition Creator** mode
2. Agree to the copyright terms
3. Upload `.glb` models from device storage
4. Scan the room to recognize planes
5. Place **two calibration reference points** on the floor in recognizable locations in the room
6. Place objects on the planes, scale and rotate them
7. Save the exhibition

### Exhibition Viewer

1. Select **Exhibition Viewer** mode
2. Load the exhibition file
3. Place calibration reference points **in the same locations** as the creator
4. View the exhibition by walking around the objects

---

## Recommended limits for .glb models

| Parameter | Recommended limit |
|------------|------------------------|
| Total size of model files in the exhibition | ≤ 100 MB |
| Number of triangles per model | ≤ 25,000 |
| Total number of triangles in the scene | ≤ 250,000 |

---

## Author

**Krišjānis Vītiņš**

Riga Technical University — Faculty of Computer Science, Information Technology and Energy

Bachelor's thesis, 2026
