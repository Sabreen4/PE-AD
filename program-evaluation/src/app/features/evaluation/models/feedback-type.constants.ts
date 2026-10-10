export const FEEDBACK_TYPE_CODES = {
  GREEN_FLAG: 'GREEN_FLAG',
  RED_FLAG: 'RED_FLAG',
} as const;

export type FeedbackTypeCode = (typeof FEEDBACK_TYPE_CODES)[keyof typeof FEEDBACK_TYPE_CODES];

export interface FeedbackTypeOption {
  code: FeedbackTypeCode;
  display: string;
}

export const FEEDBACK_TYPE_OPTIONS: FeedbackTypeOption[] = [
  { code: FEEDBACK_TYPE_CODES.GREEN_FLAG, display: 'Green Flag' },
  { code: FEEDBACK_TYPE_CODES.RED_FLAG, display: 'Red Flag' },
];
