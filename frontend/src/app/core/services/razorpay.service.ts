import { Injectable } from '@angular/core';

declare var Razorpay: any;

export interface RazorpayOptions {
  key: string;
  amount: number;
  currency: string;
  name: string;
  description: string;
  order_id: string;
  handler: (response: any) => void;
  prefill?: {
    name?: string;
    email?: string;
    contact?: string;
  };
  theme?: {
    color?: string;
  };
  modal?: {
    ondismiss?: () => void;
  };
}

@Injectable({
  providedIn: 'root'
})
export class RazorpayService {
  constructor() {}

  /**
   * Opens Razorpay payment modal
   */
  openPaymentModal(options: RazorpayOptions): void {
    const rzp = new Razorpay(options);
    rzp.open();
  }

  /**
   * Checks if Razorpay script is loaded
   */
  isRazorpayLoaded(): boolean {
    return typeof Razorpay !== 'undefined';
  }
}
