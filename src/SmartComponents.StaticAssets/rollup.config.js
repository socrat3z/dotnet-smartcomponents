import typescript from '@rollup/plugin-typescript';
import terser from '@rollup/plugin-terser';
import { nodeResolve } from '@rollup/plugin-node-resolve';

const isDebug = process.env.BUILD === 'Debug';

const config = {
    input: 'typescript/main.ts',
    output: {
        format: 'es',
        sourcemap: isDebug ? 'inline' : false,
        plugins: isDebug ? [] : [
            terser({
                compress: {
                    drop_console: false,
                    passes: 2
                },
                mangle: {
                    properties: false
                },
                format: {
                    comments: false
                }
            })
        ]
    },
    plugins: [
        nodeResolve({
            browser: true,
            preferBuiltins: false
        }),
        typescript({
            noEmitOnError: true,
            tsconfig: './tsconfig.json',
            sourceMap: isDebug,
            inlineSources: isDebug
        })
    ],
    onwarn(warning, warn) {
        // Suppress certain warnings
        if (warning.code === 'THIS_IS_UNDEFINED') return;
        warn(warning);
    }
};

export default config;
