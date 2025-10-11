import { loadStripe } from '@stripe/stripe-js';

// For development, we'll use Stripe's test publishable key
// In production, this should come from environment variables
const STRIPE_PUBLISHABLE_KEY = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || 'pk_test_51234567890abcdef'; // Replace with actual test key

export const stripePromise = loadStripe(STRIPE_PUBLISHABLE_KEY);

export const STRIPE_CONFIG = {
  publishableKey: STRIPE_PUBLISHABLE_KEY,
  appearance: {
    theme: 'stripe' as const,
    variables: {
      colorPrimary: '#0570de',
      colorBackground: '#ffffff',
      colorText: '#30313d',
      colorDanger: '#df1b41',
      fontFamily: 'Ideal Sans, system-ui, sans-serif',
      spacingUnit: '2px',
      borderRadius: '4px',
    },
  },
  options: {
    layout: {
      type: 'tabs' as const,
      defaultCollapsed: false,
    },
  },
};
