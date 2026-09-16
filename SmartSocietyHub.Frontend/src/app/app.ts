import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ConfirmHost } from './shared/ui/confirm-dialog/confirm-host';
import { ToastHost } from './shared/ui/toast/toast-host';

@Component({
  imports: [RouterOutlet, ToastHost, ConfirmHost],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {}
