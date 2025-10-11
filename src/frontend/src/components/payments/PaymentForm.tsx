import React, { useState } from 'react';
import { 
  PaymentElement, 
  useStripe, 
  useElements 
} from '@stripe/react-stripe-js';
import { Button } from '../ui/Button';
import { paymentService } from '../../services/paymentService';
// import type { PaymentFormData } from '../../types';

interface PaymentFormProps {
  amount: number;
  currency?: string;
  description?: string;
  onSuccess?: (payment: any) => void;
  onError?: (error: string) => void;
}

export const PaymentForm: React.FC<PaymentFormProps> = ({
  amount,
  currency = 'USD',
  description,
  onSuccess,
  onError,
}) => {
  const stripe = useStripe();
  const elements = useElements();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      // Create payment intent
      const { clientSecret } = await paymentService.createPaymentIntent(amount, currency);

      // Confirm payment with Stripe
      const { error: stripeError, paymentIntent } = await stripe.confirmPayment({
        elements,
        clientSecret,
        confirmParams: {
          return_url: `${window.location.origin}/payment-success`,
        },
        redirect: 'if_required',
      });

      if (stripeError) {
        setError(stripeError.message || 'Payment failed');
        onError?.(stripeError.message || 'Payment failed');
      } else if (paymentIntent?.status === 'succeeded') {
        // Create payment record in our system
        const payment = await paymentService.createPayment({
          amount: paymentIntent.amount / 100, // Convert from cents
          currency: paymentIntent.currency,
          paymentMethodId: paymentIntent.payment_method as string,
          description,
        });

        onSuccess?.(payment);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Payment failed';
      setError(errorMessage);
      onError?.(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="bg-white p-6 rounded-lg border border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Payment Details
        </h3>
        
        <div className="mb-4">
          <div className="flex justify-between items-center mb-2">
            <span className="text-sm text-gray-600">Amount</span>
            <span className="font-semibold">
              {currency} {(amount / 100).toFixed(2)}
            </span>
          </div>
          {description && (
            <div className="text-sm text-gray-500">{description}</div>
          )}
        </div>

        <div className="mb-6">
          <PaymentElement 
            options={{
              layout: 'tabs',
            }}
          />
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
            <p className="text-sm text-red-600">{error}</p>
          </div>
        )}

        <Button
          type="submit"
          disabled={!stripe || isLoading}
          className="w-full"
          variant="primary"
        >
          {isLoading ? 'Processing...' : `Pay ${currency} ${(amount / 100).toFixed(2)}`}
        </Button>
      </div>
    </form>
  );
};
