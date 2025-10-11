import React, { useState } from 'react';
import { StripeProvider } from '../components/payments/StripeProvider';
import { SubscriptionForm } from '../components/payments/SubscriptionForm';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';

const SUBSCRIPTION_PLANS = [
  {
    id: 'basic',
    name: 'Basic Plan',
    price: 999, // $9.99 in cents
    currency: 'USD',
    features: ['Basic features', 'Email support', '5GB storage'],
  },
  {
    id: 'pro',
    name: 'Pro Plan',
    price: 1999, // $19.99 in cents
    currency: 'USD',
    features: ['All basic features', 'Priority support', '50GB storage', 'Advanced analytics'],
  },
  {
    id: 'enterprise',
    name: 'Enterprise Plan',
    price: 4999, // $49.99 in cents
    currency: 'USD',
    features: ['All pro features', '24/7 support', 'Unlimited storage', 'Custom integrations'],
  },
];

export const SubscriptionPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedPlan, setSelectedPlan] = useState(SUBSCRIPTION_PLANS[0]);

  const handleSubscriptionSuccess = (subscription: any) => {
    console.log('Subscription successful:', subscription);
    navigate('/subscription-success', { 
      state: { 
        subscriptionId: subscription.id,
        planId: subscription.planId,
        status: subscription.status 
      } 
    });
  };

  const handleSubscriptionError = (error: string) => {
    console.error('Subscription failed:', error);
    alert(`Subscription failed: ${error}`);
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-4xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Choose Your Plan</h1>
          <p className="mt-2 text-gray-600">
            Select the perfect plan for your needs
          </p>
        </div>

        {/* Plan Selection */}
        <div className="grid md:grid-cols-3 gap-6 mb-8">
          {SUBSCRIPTION_PLANS.map((plan) => (
            <div
              key={plan.id}
              className={`p-6 rounded-lg border-2 cursor-pointer transition-all ${
                selectedPlan.id === plan.id
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 bg-white hover:border-gray-300'
              }`}
              onClick={() => setSelectedPlan(plan)}
            >
              <div className="text-center">
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {plan.name}
                </h3>
                <div className="mb-4">
                  <span className="text-3xl font-bold text-gray-900">
                    ${(plan.price / 100).toFixed(2)}
                  </span>
                  <span className="text-gray-600">/month</span>
                </div>
                <ul className="space-y-2 text-sm text-gray-600">
                  {plan.features.map((feature, index) => (
                    <li key={index} className="flex items-center">
                      <svg className="w-4 h-4 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                      </svg>
                      {feature}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          ))}
        </div>

        {/* Subscription Form */}
        <div className="max-w-md mx-auto">
          <StripeProvider>
            <SubscriptionForm
              planId={selectedPlan.id}
              amount={selectedPlan.price}
              currency={selectedPlan.currency}
              onSuccess={handleSubscriptionSuccess}
              onError={handleSubscriptionError}
            />
          </StripeProvider>
        </div>

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
