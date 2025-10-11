import React, { useState } from 'react';
import { 
  PaymentElement, 
  useStripe, 
  useElements 
} from '@stripe/react-stripe-js';
import { Button } from '../ui/Button';
import { paymentService } from '../../services/paymentService';
// import type { SubscriptionFormData } from '../../types';

interface SubscriptionFormProps {
  planId: string;
  amount: number;
  currency?: string;
  onSuccess?: (subscription: any) => void;
  onError?: (error: string) => void;
}

export const SubscriptionForm: React.FC<SubscriptionFormProps> = ({
  planId,
  amount,
  currency = 'USD',
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
      // Create subscription with Stripe
      const { error: stripeError, paymentMethod } = await stripe.createPaymentMethod({
        elements,
        params: {
          billing_details: {
            name: 'Customer Name', // This should come from user context
          },
        },
      });

      if (stripeError) {
        setError(stripeError.message || 'Payment method creation failed');
        onError?.(stripeError.message || 'Payment method creation failed');
        return;
      }

      if (paymentMethod) {
        // Create subscription in our system
        const subscription = await paymentService.createSubscription({
          planId,
          paymentMethodId: paymentMethod.id,
        });

        onSuccess?.(subscription);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Subscription creation failed';
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
          Subscribe to Plan
        </h3>
        
        <div className="mb-4">
          <div className="flex justify-between items-center mb-2">
            <span className="text-sm text-gray-600">Plan</span>
            <span className="font-semibold">{planId}</span>
          </div>
          <div className="flex justify-between items-center mb-2">
            <span className="text-sm text-gray-600">Amount</span>
            <span className="font-semibold">
              {currency} {(amount / 100).toFixed(2)}/month
            </span>
          </div>
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
          {isLoading ? 'Creating Subscription...' : `Subscribe for ${currency} ${(amount / 100).toFixed(2)}/month`}
        </Button>
      </div>
    </form>
  );
};
