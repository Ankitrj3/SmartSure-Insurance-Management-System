import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../../core/services/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css']
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: (Toast & { id: number })[] = [];
  private subscription?: Subscription;
  private nextId = 0;

  constructor(private toastService: ToastService) {}

  ngOnInit() {
    this.subscription = this.toastService.toast$.subscribe(toast => {
      const id = this.nextId++;
      const toastWithId = { ...toast, id };
      this.toasts.push(toastWithId);

      setTimeout(() => {
        this.removeToast(id);
      }, toast.duration || 3000);
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  removeToast(id: number) {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }
}
