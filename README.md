## 🪶🪕🎻 **A.R.M.O.N.I.A.**

**Advanced Recording & Media Organizer for Natural Integrated Audio**

A.R.M.O.N.I.A. (Italian for *Harmony*) is an open-source digital audio environment built to bridge the gap between 
exorbitant professional digital audio workstations (DAWs) and free, but more complex, audio tools.
Designed for independent artists, songwriters, and smaller producers who want *control* without *clutter*.

### *In Pre-Development*
---

## 🎯 **Purpose**

### Problems Armonia Aims to Solve

|   #   | Problem                    | Description                                                                                                                                                                              |
| :---: | :------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **1** | **Scalability**            | Most DAWs are massive and over-engineered for casual users and beginners. Armonia focuses on minimal, intuitive scaling.                                                                |
| **2** | **Price Barrier**          | No one should need a $300 license to access EQ editing or MIDI controls. Armonia aims to provide professional-grade core tools entirely free.                                           |
| **3** | **Solo Workflow**          | Designed for artists who write, sing, and record, not just producers. Includes an integrated lyrics editor and simple track management.                                                 |
| **4** | **Cross-Platform**         | An eventual React iOS companion app to allow editing and playback from your phone.                                                                                                      |
| **5** | **Project Storage**        | Future plans include a limited cloud database for polished projects. Users can store, retrieve, and remix anywhere.                                                                     |
| **6** | **Localization**           | An eventual Language localization for both English and l'Italiano. Dopotutto il nome è italiano.                                                                                        |


## 🧠 **Design Philosophy**

Armonia aims to emulate the **feel of coding in a music IDE**; a calm, expressive space where your workflow has space to breathe.
UI is focused on clarity, warmth, and familiarity. Designed with grassroots inspiration and minimalism.

---

## 🔍 **Tech Stack**

| Layer                | Technology                                        |
| :------------------- | :------------------------------------------------ |
| **Frontend / UI**    | WPF (.NET 8), XAML, C# MVVM                       |
| **Audio Processing** | NAudio (WasapiCapture), custom waveform rendering |
| **Storage**          | Local `/projects` structure → future cloud API    |

---

## 🎼 **Development Progress**

#### **🎛 Interface & Layout**

* Fully designed **Record Page**, **Composer**, and **Lyrics** sections.
* Responsive **Record–Composer–Lyrics** layout with a dynamic divider.
* Record and Composer **Control Buttons** (Play, Stop, Record) with active-state animations.
* Working **toolbar logic** and animation smoothing for clean transitions and scaling.
* Polished **Italian-inspired beige/brown UI theme** consistent across views.

#### **Audio & Visualization**

* Live audio capture through **WasapiCapture** integrated on RecordPage.
* Functional **waveform visualizer** (realistic gold bar wave display) that reacts in real time to input amplitude using my own C# logic (thanks to NAudio).
* Core **waveform rendering pipeline** complete, I use for both live recording and static display in Composer.
* Waveform coloration, gradient, and scaling now visually refined and tested.

#### **Composer Framework**

* Semi-functional **Composer section** with dynamic lanes and timeline integration.
* Configurable **horizontal BPM wheel** with realistic knob UI.
* Dynamic **track rows** with mute/solo buttons, realistic fader sliders, and add/remove logic.
* Hook system linking the **RecordPage output** and **Composer** base architecture for eventual “Push-to-Composer” feature.

#### **Architecture & Framework**

* MVVM architecture with `BindableBase`, `ClipViewModel`, `TrackViewModel`, and `ComposerViewModel`.
* Modular control structure (`TrackRow`, `TrackAddRow`, `TimelineControl`, `ComposerControl`) ensuring scalability and clear separation.
* Smooth animation timing, toolbar interaction hooks, and refined mouse event control throughout.

---

##  **Planned / In Progress**

* Time-synchronized **tick grid** and scrolling logic aligned with BPM tempo.
* Logic for dragging, splicing, resizing, and deleting waveform clips in each lane. 
* MIDI instrument support
* Integrated lyrics editor (Audio and Text matching)
* Project sync
* Theme engine (light, dark, rustic modes)

---
