import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button';

export const SubscriptionSuccessPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  
  const subscriptionData = location.state as {
    subscriptionId: string;
    planId: string;
    status: string;
  } | null;

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="max-w-md mx-auto text-center">
        <div className="bg-white rounded-lg shadow-sm p-8">
          <div className="mb-6">
            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
              <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">
              Subscription Created!
            </h1>
            <p className="text-gray-600">
              Your subscription has been set up successfully.
            </p>
          </div>

          {subscriptionData && (
            <div className="bg-gray-50 rounded-lg p-4 mb-6 text-left">
              <h3 className="font-semibold text-gray-900 mb-2">Subscription Details</h3>
              <div className="space-y-1 text-sm text-gray-600">
                <div className="flex justify-between">
                  <span>Subscription ID:</span>
                  <span className="font-mono">{subscriptionData.subscriptionId}</span>
                </div>
                <div className="flex justify-between">
                  <span>Plan:</span>
                  <span className="capitalize">{subscriptionData.planId}</span>
                </div>
                <div className="flex justify-between">
                  <span>Status:</span>
                  <span className="capitalize">{subscriptionData.status}</span>
                </div>
              </div>
            </div>
          )}

          <div className="space-y-3">
            <Button
              variant="primary"
              onClick={() => navigate('/dashboard')}
              className="w-full"
            >
              Back to Dashboard
            </Button>
            <Button
              variant="secondary"
              onClick={() => navigate('/subscriptions')}
              className="w-full"
            >
              Manage Subscriptions
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};
