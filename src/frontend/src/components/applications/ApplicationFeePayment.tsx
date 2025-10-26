import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { loadStripe, type StripeElementsOptions } from '@stripe/stripe-js';
import { CreditCard, Lock } from 'lucide-react';
import axios from 'axios';

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || '');

interface ApplicationFeePaymentProps {
  applicationId: string;
  amount: number;
  currency: string;
  onSuccess?: () => void;
  onCancel?: () => void;
}

const PaymentForm = ({ applicationId, amount, currency, onSuccess, onCancel }: ApplicationFeePaymentProps) => {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [processing, setProcessing] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setProcessing(true);
    setError(null);

    try {
      const { error: submitError } = await elements.submit();
      if (submitError) {
        setError(submitError.message || 'Payment failed');
        setProcessing(false);
        return;
      }

      // Get client secret from backend
      const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111';
      const response = await axios.post(
        `${API_BASE_URL}/api/applications/${applicationId}/pay-fee`,
        {},
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`,
          },
        }
      );

      const { clientSecret } = response.data;

      // Confirm the payment
      const { error: confirmError } = await stripe.confirmPayment({
        elements,
        clientSecret,
        confirmParams: {
          return_url: `${window.location.origin}/applications/payment-success`,
        },
      });

      if (confirmError) {
        setError(confirmError.message || 'Payment failed');
      } else {
        if (onSuccess) {
          onSuccess();
        } else {
          navigate('/applications/my');
        }
      }
    } catch (err) {
      console.error('Payment error:', err);
      setError('Failed to process payment. Please try again.');
    } finally {
      setProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div className="flex items-center gap-2 mb-2">
          <Lock className="w-5 h-5 text-blue-600" />
          <span className="font-semibold text-blue-900">Secure Payment</span>
        </div>
        <p className="text-sm text-blue-800">
          Your payment information is encrypted and secure. We never store your card details.
        </p>
      </div>

      <div className="bg-gray-50 rounded-lg p-4">
        <div className="flex justify-between items-center mb-2">
          <span className="text-gray-700">Application Fee</span>
          <span className="text-2xl font-bold text-gray-900">
            {currency} {amount.toLocaleString()}
          </span>
        </div>
        <p className="text-sm text-gray-600">
          This fee is non-refundable and covers the cost of processing your rental application.
        </p>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Payment Details
        </label>
        <PaymentElement />
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
          {error}
        </div>
      )}

      <div className="flex gap-4">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={processing}
            className="flex-1 px-6 py-3 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
        )}
        <button
          type="submit"
          disabled={!stripe || processing}
          className="flex-1 flex items-center justify-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 transition-colors font-semibold disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <CreditCard className="w-5 h-5" />
          <span>{processing ? 'Processing...' : `Pay ${currency} ${amount.toLocaleString()}`}</span>
        </button>
      </div>
    </form>
  );
};

export const ApplicationFeePayment = (props: ApplicationFeePaymentProps) => {
  const [clientSecret, setClientSecret] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    initializePayment();
  }, []);

  const initializePayment = async () => {
    try {
      setLoading(true);
      const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111';
      const response = await axios.post(
        `${API_BASE_URL}/api/applications/${props.applicationId}/pay-fee`,
        {},
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`,
          },
        }
      );

      setClientSecret(response.data.clientSecret);
    } catch (err) {
      console.error('Failed to initialize payment:', err);
      setError('Failed to initialize payment. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-600">Initializing payment...</div>
      </div>
    );
  }

  if (error || !clientSecret) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
        {error || 'Failed to load payment form'}
      </div>
    );
  }

  const options: StripeElementsOptions = {
    clientSecret,
    appearance: {
      theme: 'stripe',
    },
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center gap-2 mb-6">
        <CreditCard className="w-6 h-6 text-blue-600" />
        <h2 className="text-2xl font-bold text-gray-900">Pay Application Fee</h2>
      </div>
      <Elements stripe={stripePromise} options={options}>
        <PaymentForm {...props} />
      </Elements>
    </div>
  );
};

