import { Component, inject } from '@angular/core';

import { ConfirmService } from './confirm.service';

@Component({
  selector: 'app-confirm-host',
  standalone: true,
  templateUrl: './confirm-host.html',
  styleUrl: './confirm-host.scss',
})
export class ConfirmHost {
  protected readonly confirmService = inject(ConfirmService);
}
