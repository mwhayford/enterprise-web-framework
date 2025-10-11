// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React from 'react';
import { PaymentMethodsList } from '../components/payments/PaymentMethodsList';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';

export const PaymentMethodsPage: React.FC = () => {
  const navigate = useNavigate();

  const handleSelectPaymentMethod = (paymentMethod: any) => {
    console.log('Selected payment method:', paymentMethod);
    // Handle payment method selection logic here
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-2xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Payment Methods</h1>
          <p className="mt-2 text-gray-600">
            Manage your saved payment methods
          </p>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6">
          <PaymentMethodsList
            onSelectPaymentMethod={handleSelectPaymentMethod}
            showAddButton={true}
          />
        </div>

        <div className="text-center mt-6 space-x-4">
          <Button
            variant="secondary"
            onClick={() => navigate('/dashboard')}
          >
            Back to Dashboard
          </Button>
          <Button
            variant="primary"
            onClick={() => navigate('/payment')}
          >
            Make a Payment
          </Button>
        </div>
      </div>
    </div>
  );
};
