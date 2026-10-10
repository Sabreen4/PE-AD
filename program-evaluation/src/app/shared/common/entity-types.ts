export type FieldType = 'text' | 'number' | 'select' | 'checkbox' | 'textarea' | 'date';

export interface SelectOption {
  value: string | number;
  label: string;
}

export interface FormFieldConfig {
  key: string;
  label: string;
  type: FieldType;
  required?: boolean;
  options?: SelectOption[];
  hint?: string;
  placeholder?: string;
  disabled?: boolean;
}

export interface TableColumnConfig<T = Record<string, string | number | boolean | null>> {
  key: keyof T & string;
  label: string;
  mono?: boolean;
  formatter?: (row: T) => string;
}
