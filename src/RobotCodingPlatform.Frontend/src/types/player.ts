export interface NullableUser {
    user: User;
    hasUser: boolean;
}

export interface User {
    id: string;
    name: string;
    email: string;
    role: string;
    createdAt : string;
}
