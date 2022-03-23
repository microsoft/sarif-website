const OFF = 0, ERROR = 2

module.exports = {
    overrides: [
        {
            files: ["src/**/*.ts{,x}"],
            extends: [
                "eslint:recommended",
            ],
            rules: {
                "new-cap": ERROR,
                "no-console": [ERROR, {allow: ["error", "warn"]}],
                "no-throw-literal": ERROR,
                "no-var": ERROR,
                "prefer-const": ERROR,

                "eqeqeq": ERROR,
                "filenames/match-regex": [ERROR, "^([a-z0-9]+)([A-Z][a-z0-9]+)*(\.(config|d|layouts|spec))?$"],
                "header/header": [ERROR, "line", [
                    " Copyright (c) Microsoft Corporation. All rights reserved.",
                    " Licensed under the MIT License.",
                ]],
                "indent": [ERROR, 4, { "SwitchCase": 1 }],
                "no-trailing-spaces": ERROR,
                "quotes": [ERROR, "single", {"allowTemplateLiterals": true}],
                "semi": ERROR,
                
                // Exceptions with Justifications.
                "no-undef": OFF, // Requires too many exception account for Mocha, Node.js and browser globals. Typescript also already checks for this.
            },
        }
    ],
    parserOptions: {
        ecmaVersion: 6,
        sourceType: "module",
        "ecmaFeatures": {
            "jsx": true
        },
    },
    plugins: [
        "filenames",
        "header",
    ],
}
