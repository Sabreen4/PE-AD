export function buildPath<T extends object>(template: string, params: T): string {
  const values = params as {} as Record<string, string | number>;
  return Object.keys(values).reduce(
    (path, key) => path.replace(`:${key}`, encodeURIComponent(String(values[key]))),
    template
  );
}