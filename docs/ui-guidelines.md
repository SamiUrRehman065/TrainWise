# TrainWise UI Design System & Guidelines

## 🌌 Theme Overview: **Deep Navy Glassmorphism**
TrainWise uses a premium, dark-mode-first aesthetic designed for high-performance data analysis. The design system prioritizes visual depth, typographic clarity, and smooth interactivity.

---

## 🎨 Color Palette

### Base Surfaces
- **App Background**: `#0d1117` (Deep Space Black/Navy)
- **Surface Elevation**: `rgba(22, 27, 39, 0.7)` (Glassmorphic Navy)
- **Border Subtle**: `rgba(255, 255, 255, 0.08)`
- **Border Active**: `rgba(129, 140, 248, 0.3)`

### Accents
- **Primary Gradient**: `linear-gradient(135deg, #818cf8 0%, #6366f1 100%)` (Indigo to Violet)
- **Cyan Accent**: `#22d3ee` (Electric Blue - used for highlights and code)
- **Success**: `#34d399` (Emerald Green)
- **Warning**: `#fbbf24` (Amber)
- **Danger**: `#ef4444` (Rose Red)

---

## 📐 Component Architecture

### 1. Cards (`tw-card`)
All containers must use the glassmorphic card style:
- **Background**: `rgba(255, 255, 255, 0.03)`
- **Backdrop Blur**: `blur(20px)`
- **Border Radius**: `24px`
- **Border**: `1px solid rgba(255, 255, 255, 0.05)`

### 2. Stat Cards (`tw-stat-card`)
Used for displaying KPIs and model metrics:
- **Large Number**: `2.5rem` font size, bold, usually `#ffffff` or `text-gradient`.
- **Label**: `0.8rem`, uppercase, letter-spacing `1px`, color `var(--text-muted)`.

### 3. Buttons
- **Primary (`tw-btn-primary`)**: Pill-shaped (`border-radius: 50px`), gradient background, shadow on hover.
- **Ghost (`tw-btn-ghost`)**: Transparent background, subtle border, shifts background color on hover.

### 4. Typography
- **Headings**: Modern Sans Serif (Inter / Outfit). Font weight `700` or `800`.
- **Monospace**: Used for IDs and data values (JetBrains Mono / Fira Code).
- **Gradient Text**: Apply `.text-gradient` to primary headings for branding.

---

## ✨ Motion & Interactions

### Entrance Animations
Every page and card should use the `.animate-fade-up` class:
```css
@keyframes fadeUp {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}
```

### Hover States
- **Cards**: Subtle scale up (`scale(1.01)`) or border-color shift.
- **Buttons**: Glow effect using `box-shadow` with the accent color.

---

## 🛠️ Global CSS Tokens
All styles are centralized in `wwwroot/css/app.css`. Use the following CSS variables for consistency:
- `--bg-dark`: `#0d1117`
- `--accent-grad`: `linear-gradient(...)`
- `--text-h1`: `#ffffff`
- `--text-muted`: `#94a3b8`

---

## 📝 Developer Checklist for New Pages
1. Wrap page content in `<div class="page-container animate-fade-up">`.
2. Use `<section class="tw-section-header">` for titles.
3. Place data in `tw-card` or `tw-stat-card` layouts.
4. Ensure all buttons are either `tw-btn-primary` or `tw-btn-ghost`.
5. Use `tw-badge` for status or categories.
