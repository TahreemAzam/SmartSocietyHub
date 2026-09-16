import { Component, Input } from '@angular/core';

// Horizon Residencia's mark: a skyline of three rooflines resting on a
// horizon line, with a small sun/accent above the tallest peak. Reads
// as both "residential community" and "horizon" at a glance, and scales
// cleanly from sidebar size up to the login screen.
@Component({
  selector: 'app-brand-mark',
  standalone: true,
  templateUrl: './brand-mark.html',
  styleUrl: './brand-mark.scss',
})
export class BrandMark {
  @Input() size = 40;
}
