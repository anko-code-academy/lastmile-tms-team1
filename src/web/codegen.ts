import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  schema: process.env.NEXT_PUBLIC_API_URL
    ? `${process.env.NEXT_PUBLIC_API_URL}/api/graphql`
    : 'http://localhost:5000/api/graphql',
  documents: 'src/graphql/documents/**/*.graphql',
  generates: {
    'src/graphql/generated/': {
      preset: 'client',
    },
  },
  ignoreNoDocuments: true,
};

export default config;
