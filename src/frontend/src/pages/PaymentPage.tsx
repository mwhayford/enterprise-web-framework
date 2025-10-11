import React, { useState } from 'react';
import { StripeProvider } from '../components/payments/StripeProvider';
import { PaymentForm } from '../components/payments/PaymentForm';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';

export const PaymentPage: React.FC = () => {
  const navigate = useNavigate();
  const [amount, setAmount] = useState(2000); // $20.00 in cents
  const [description, setDescription] = useState('Sample payment');
  // const [isProcessing, setIsProcessing] = useState(false);

  const handlePaymentSuccess = (payment: any) => {
    console.log('Payment successful:', payment);
    navigate('/payment-success', { 
      state: { 
        paymentId: payment.id,
        amount: payment.amount,
        status: payment.status 
      } 
    });
  };

  const handlePaymentError = (error: string) => {
    console.error('Payment failed:', error);
    alert(`Payment failed: ${error}`);
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-md mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Make a Payment</h1>
          <p className="mt-2 text-gray-600">
            Complete your payment securely with Stripe
          </p>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Payment Details
          </h2>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Amount (USD)
              </label>
              <input
                type="number"
                value={amount / 100}
                onChange={(e) => setAmount(Math.max(0, parseFloat(e.target.value) * 100))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                min="0"
                step="0.01"
                placeholder="0.00"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description
              </label>
              <input
                type="text"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Payment description"
              />
            </div>
          </div>
        </div>

        <StripeProvider>
          <PaymentForm
            amount={amount}
            currency="USD"
            description={description}
            onSuccess={handlePaymentSuccess}
            onError={handlePaymentError}
          />
        </StripeProvider>

        <div className="text-center mt-6">
          <Button
            variant="secondary"
            onClick={() => navigate('/dashboard')}
          >
            Back to Dashboard
          </Button>
        </div>
      </div>
    </div>
  );
};
